using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Wolverine;
using Wolverine.RabbitMQ;
using OpenIddict.Abstractions;
using System.Security.Cryptography.X509Certificates;
using OidcServer.Services;
using FinanceTracking.Contracts.Events;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);


var certPath = "/https/identityserver.pfx";
var certPassword = builder.Configuration["ASPNETCORE_Kestrel:Certificates:Default:Password"];
var certificate = new X509Certificate2(certPath, certPassword, 
    X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.EphemeralKeySet);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.UseOpenIddict();
});

builder.Services.AddDataProtection()
    .PersistKeysToDbContext<ApplicationDbContext>();

builder.Host.UseWolverine(opts =>
{
    opts.UseRabbitMq(rabbit =>
    {
        rabbit.HostName = builder.Configuration["RabbitMq:Host"];
        rabbit.UserName = builder.Configuration["RabbitMq:Username"];
        rabbit.Password = builder.Configuration["RabbitMq:Password"];
    }).AutoProvision();

    opts.PublishMessage<UserCreatedEvent>().ToRabbitQueue("user-created");
    opts.PublishMessage<UserDeletedEvent>().ToRabbitQueue("user-deleted");
});

builder.Services.AddScoped<UserEventPublisher>();

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddTransient<IEmailSender, DummyEmailSender>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
});

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore().UseDbContext<ApplicationDbContext>();
    })
    .AddServer(options =>
    {
        options.SetAuthorizationEndpointUris("/connect/authorize")
                .SetEndSessionEndpointUris("/connect/logout")
               .SetTokenEndpointUris("/connect/token")
               .SetUserInfoEndpointUris("/connect/userinfo");

        options.DisableAccessTokenEncryption();

        options.AllowAuthorizationCodeFlow()
                .RequireProofKeyForCodeExchange()
                .AllowRefreshTokenFlow();

        options.AddSigningCertificate(certificate)
               .AddEncryptionCertificate(certificate);

        options.RegisterScopes(
            OpenIddictConstants.Scopes.OpenId, 
            OpenIddictConstants.Scopes.Profile,
            OpenIddictConstants.Scopes.Email,
            builder.Configuration["ApiSettings:ApiResourceName"]
        );

        options.UseAspNetCore()
               .EnableAuthorizationEndpointPassthrough()
               .EnableTokenEndpointPassthrough()
               .EnableEndSessionEndpointPassthrough()
               .EnableUserInfoEndpointPassthrough();
    });

builder.Services.AddRazorPages();
builder.Services.AddControllers();

builder.Services.AddHostedService<Worker>();

var app = builder.Build();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

app.Run();