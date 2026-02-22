using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;

public class Worker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public Worker(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        var clientId = _configuration["OidcClients:Nuxt:ClientId"] ?? "nuxt-client";
        var displayName = _configuration["OidcClients:Nuxt:DisplayName"] ?? "Nuxt Frontend";
        var redirectUri = _configuration["OidcClients:Nuxt:RedirectUri"];

        if (string.IsNullOrEmpty(redirectUri))
        {
            throw new InvalidOperationException("OidcClients:Nuxt:RedirectUri configuration is missing.");
        }

        // DEV ONLY, REMOVE LATER!!!
        var client = await manager.FindByClientIdAsync(clientId);
        if (client is not null)
        {
            // Delete the old client so we can update it
            await manager.DeleteAsync(client);
        }

        if (await manager.FindByClientIdAsync(clientId) is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = clientId,
                ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
                DisplayName = displayName,
                RedirectUris = { new Uri(redirectUri) },
                // Temporary, change to actual PostLogoutRedirectUris later
                PostLogoutRedirectUris = { new Uri(redirectUri) },
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.Endpoints.EndSession,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Roles
                }
            });
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}