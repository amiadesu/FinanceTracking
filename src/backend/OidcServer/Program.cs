using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using OpenIddict.Abstractions;
using OidcServer.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.UseOpenIddict();
});

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.User.RequireUniqueEmail = true;
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
                .AllowRefreshTokenFlow();;
        options.AddDevelopmentEncryptionCertificate().AddDevelopmentSigningCertificate();

        options.RegisterScopes(
            OpenIddictConstants.Scopes.OpenId, 
            OpenIddictConstants.Scopes.Profile,
            OpenIddictConstants.Scopes.Email
        );

        options.UseAspNetCore()
               .EnableAuthorizationEndpointPassthrough()
               .EnableTokenEndpointPassthrough()
               .EnableEndSessionEndpointPassthrough();
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

app.Run();