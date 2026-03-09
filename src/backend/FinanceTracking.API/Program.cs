using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Wolverine;
using Wolverine.RabbitMQ;
using FluentValidation;
using FinanceTracking.API.Validators;
using FinanceTracking.API.Data;
using FinanceTracking.API.Models;
using FinanceTracking.Contracts.Events;
using FinanceTracking.API.Services;
using System.Reflection;
using FinanceTracking.API.Filters;
using FinanceTracking.API.DTOs;
using FinanceTracking.API.Utils;

System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

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

builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IGroupHistoryService, GroupHistoryService>();
builder.Services.AddScoped<IGroupHistoryExportService, GroupHistoryExportService>();
builder.Services.AddScoped<IGroupInvitationService, GroupInvitationService>();
builder.Services.AddScoped<IBudgetGoalService, BudgetGoalService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ReceiptService>();
builder.Services.AddScoped<SellerService>();
builder.Services.AddScoped<ProductDataService>();
builder.Services.AddScoped<IGroupMemberService, GroupMemberService>();
builder.Services.AddScoped<StatisticsService>();

builder.Services.AddHttpClient("MlServiceClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["MlService:BaseUrl"] ?? "http://ml_service:8000");
});

builder.Services.AddHostedService<MlCategorySyncService>();

builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddTransient(typeof(ValidationFilter<>));

builder.Services.AddSingleton<IPendingPredictionRequests, PendingPredictionRequests>();

var mlRequestQueue = builder.Configuration["RabbitMq:MlRequestQueue"]
    ?? throw new InvalidOperationException("Configuration key 'RabbitMq:MlRequestQueue' is required but was not set.");

var mlReplyQueue = builder.Configuration["RabbitMq:MlReplyQueue"]
    ?? throw new InvalidOperationException("Configuration key 'RabbitMq:MlReplyQueue' is required but was not set.");

builder.Host.UseWolverine(opts =>
{
    opts.UseRabbitMq(rabbit =>
    {
        rabbit.HostName = builder.Configuration["RabbitMq:Host"];
        rabbit.UserName = builder.Configuration["RabbitMq:Username"];
        rabbit.Password = builder.Configuration["RabbitMq:Password"];
    }).AutoProvision();

    opts.ListenToRabbitQueue("user-created");
    opts.ListenToRabbitQueue("user-updated");
    opts.ListenToRabbitQueue("user-deleted");

    // Listen for ML prediction replies with the interop mapper so Wolverine
    // can deserialise the plain-JSON payload the Python service sends back.
    opts.ListenToRabbitQueue(mlReplyQueue)
        .UseInterop(new MLInteropMapper(mlReplyQueue));

    // Publish prediction requests to the ML service request queue.
    opts.PublishMessage<PredictionRequest>()
        .ToRabbitQueue(mlRequestQueue)
        .UseInterop(new MLInteropMapper(mlReplyQueue));
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