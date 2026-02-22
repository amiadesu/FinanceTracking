using System.Security.Claims;
using FinanceTracking.API.Data;
using FinanceTracking.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddMemoryCache();

// 1. Add Services
builder.Services.AddControllers();

builder.Services.AddDbContext<FinanceDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .UseSnakeCaseNamingConvention());

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        var authority = builder.Configuration["Authentication:Authority"];
        var rawIssuer = builder.Configuration["Authentication:ValidIssuer"];
        var audience = builder.Configuration["Authentication:Audience"];

        string validIssuerWithSlash = rawIssuer?.EndsWith("/") == true ? rawIssuer : $"{rawIssuer}/";

        options.Authority = authority;
        options.Audience = audience;
        options.RequireHttpsMetadata = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = validIssuerWithSlash,
            
            ValidateAudience = true,
            ValidAudience = audience,
            
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"[Auth JWT Error] {context.Exception.Message}");
                return Task.CompletedTask;
            },

            OnTokenValidated = async context =>
            {
                var dbContext = context.HttpContext.RequestServices.GetRequiredService<FinanceDbContext>();
                var cache = context.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();

                var userIdString = context.Principal?.FindFirst("sub")?.Value 
                                ?? context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (Guid.TryParse(userIdString, out Guid userId))
                {
                    var cacheKey = $"UserExists_{userId}";

                    // Only hit the database if the cache doesn't have the user
                    if (!cache.TryGetValue(cacheKey, out bool isUserInDb))
                    {
                        var userExists = await dbContext.Users.AnyAsync(u => u.Id == userId);

                        if (!userExists)
                        {
                            var email = context.Principal?.FindFirst("email")?.Value ?? "unknown@email.com";
                            var username = context.Principal?.FindFirst("preferred_username")?.Value 
                                        ?? context.Principal?.FindFirst("name")?.Value 
                                        ?? "New User";

                            var newUser = new AppUser
                            {
                                Id = userId,
                                UserName = username,
                                Email = email,
                                CreatedDate = DateTime.UtcNow,
                                UpdatedDate = DateTime.UtcNow
                            };

                            dbContext.Users.Add(newUser);

                            try
                            {
                                await dbContext.SaveChangesAsync();
                            }
                            catch (DbUpdateException)
                            {
                                // A parallel request beat us to the database insert.
                                // We can safely ignore this, as the user now exists in the database.
                            }
                        }

                        // Cache the result for 1 hour so subsequent requests are lightning fast
                        cache.Set(cacheKey, true, TimeSpan.FromHours(1));
                    }
                }
                else
                {
                    context.Fail("Token 'sub' claim is not a valid GUID.");
                }
            }
        };

        if (builder.Environment.IsDevelopment())
        {
            options.BackchannelHttpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
        }
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors("AllowNuxtApp");

app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<FinanceDbContext>();
        await context.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
