using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

public class AuthorizationController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public AuthorizationController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);

        if (!result.Succeeded)
        {
            if (Request.Query["action"] == "register")
            {
                var returnUrl = Request.PathBase + Request.Path + Request.QueryString;
                
                return RedirectToPage("/Account/Register", new { ReturnUrl = returnUrl });
            }

            return Challenge(IdentityConstants.ApplicationScheme);
        }

        var user = await _userManager.GetUserAsync(result.Principal);
        if (user == null)
        {
            return Forbid();
        }

        var identity = new ClaimsIdentity(
            TokenOptions.DefaultAuthenticatorProvider, 
            OpenIddictConstants.Claims.Name, 
            OpenIddictConstants.Claims.Role);

        identity.AddClaim(OpenIddictConstants.Claims.Subject, await _userManager.GetUserIdAsync(user));
        identity.AddClaim(OpenIddictConstants.Claims.Email, await _userManager.GetEmailAsync(user));
        identity.AddClaim(OpenIddictConstants.Claims.Name, await _userManager.GetUserNameAsync(user));

        foreach (var claim in identity.Claims)
        {
            claim.SetDestinations(claim.Type switch
            {
                // Add these claims to both the Access Token and the ID Token
                OpenIddictConstants.Claims.Name or
                OpenIddictConstants.Claims.Email or
                OpenIddictConstants.Claims.Subject
                    => new[] { OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken },

                // Default all other claims to just the Access Token
                _ => new[] { OpenIddictConstants.Destinations.AccessToken }
            });
        }

        var principal = new ClaimsPrincipal(identity);

        var allowedScopes = new[] 
        { 
            OpenIddictConstants.Scopes.OpenId, 
            OpenIddictConstants.Scopes.Email, 
            OpenIddictConstants.Scopes.Profile,
            OpenIddictConstants.Scopes.OfflineAccess,
            "my_api_resource"
        };

        principal.SetResources("my_api_resource");

        principal.SetScopes(allowedScopes.Intersect(HttpContext.GetOpenIddictServerRequest()?.GetScopes() ?? ImmutableArray<string>.Empty));

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpGet("~/connect/logout")]
    [HttpPost("~/connect/logout")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Logout()
    {
        // 1. Destroy the local ASP.NET Core Identity session (the cookie)
        await _signInManager.SignOutAsync();

        // 2. Return the OpenIddict SignOut result. 
        // This tells OpenIddict to process the end_session request and safely redirect 
        // the user back to the PostLogoutRedirectUri you registered in the Worker.
        return SignOut(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties
            {
                RedirectUri = "/"
            });
    }

    [HttpPost("~/connect/token")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ?? 
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.IsAuthorizationCodeGrantType())
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            
            var principal = authenticateResult.Principal;

            return SignIn(principal!, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
        else if (request.IsRefreshTokenGrantType())
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var principal = authenticateResult.Principal;

            return SignIn(principal!, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new NotImplementedException("The specified grant type is not implemented.");
    }

    [HttpGet("~/connect/userinfo")]
    [HttpPost("~/connect/userinfo")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public async Task<IActionResult> Userinfo()
    {
        // Extract the principal from the OpenIddict access token
        var authenticateResult = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        if (!authenticateResult.Succeeded)
        {
            return Challenge(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        // Find the user in the ASP.NET Core Identity database
        var subject = authenticateResult.Principal.GetClaim(OpenIddictConstants.Claims.Subject);
        var user = await _userManager.FindByIdAsync(subject!);
        if (user == null)
        {
            return Challenge(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        // Return the claims the frontend expects
        var claims = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            [OpenIddictConstants.Claims.Subject] = await _userManager.GetUserIdAsync(user),
            [OpenIddictConstants.Claims.Email] = await _userManager.GetEmailAsync(user) ?? string.Empty,
            [OpenIddictConstants.Claims.Name] = await _userManager.GetUserNameAsync(user) ?? string.Empty
        };

        return Ok(claims);
    }
}