using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FinanceTracking.API.Data;
using FinanceTracking.API.Models;
using FinanceTracking.API.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinanceTracking.API.Services;

public class MlCategorySyncService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<MlCategorySyncService> _logger;

    public MlCategorySyncService(IServiceProvider services, ILogger<MlCategorySyncService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting ML Category Synchronization...");
        
        try
        {
            // Give the ML container a few seconds to start
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            using var scope = _services.CreateScope();
            var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            var dbContext = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();

            var client = httpClientFactory.CreateClient("MlServiceClient");
            var response = await client.GetAsync("/categories", stoppingToken);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(stoppingToken);
                var mlData = JsonSerializer.Deserialize<MlCategoriesResponseDto>(
                    content, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                
                if (mlData?.Categories != null && mlData.Categories.Any())
                {
                    var existingSystemCategories = await dbContext.Categories
                        .Where(c => c.IsSystem)
                        .Select(c => c.Name)
                        .ToListAsync(stoppingToken);

                    var newCategories = mlData.Categories
                        .Where(c => !existingSystemCategories.Contains(c, StringComparer.OrdinalIgnoreCase))
                        .Select(c => new Category
                        {
                            Name = c,
                            ColorHex = Constants.ServiceConstants.SystemCategoryColor,
                            IsSystem = true,
                            CreatedDate = DateTime.UtcNow,
                            UpdatedDate = DateTime.UtcNow,
                            GroupId = null
                        })
                        .ToList();

                    if (newCategories.Any())
                    {
                        dbContext.Categories.AddRange(newCategories);
                        await dbContext.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation($"Successfully added {newCategories.Count} new system categories from ML Service.");
                    }
                }
            }
            else
            {
                _logger.LogWarning($"Failed to fetch categories from ML Service. Status Code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while syncing categories from ML Service.");
        }
    }
}