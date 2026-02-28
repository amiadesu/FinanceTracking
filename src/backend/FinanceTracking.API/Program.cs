using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Wolverine;
using Wolverine.RabbitMQ;
using FinanceTracking.API.Data;
using FinanceTracking.API.Models;
using FinanceTracking.Contracts.Events;
using FinanceTracking.API.Services;

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

builder.Services.AddScoped<GroupService>();
builder.Services.AddScoped<GroupHistoryService>();
builder.Services.AddScoped<GroupInvitationService>();
builder.Services.AddScoped<BudgetGoalService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<ReceiptService>();
builder.Services.AddScoped<SellerService>();

builder.Host.UseWolverine(opts =>
{
    opts.UseRabbitMq(rabbit =>
    {
        rabbit.HostName = builder.Configuration["RabbitMq:Host"];
        rabbit.UserName = builder.Configuration["RabbitMq:Username"];
        rabbit.Password = builder.Configuration["RabbitMq:Password"];
    }).AutoProvision();

    opts.ListenToRabbitQueue("user-created");
    opts.ListenToRabbitQueue("user-deleted");
});

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

app.UseMiddleware<FinanceTracking.API.Middleware.ExceptionHandlingMiddleware>();

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

app.MapControllers();

app.Run();