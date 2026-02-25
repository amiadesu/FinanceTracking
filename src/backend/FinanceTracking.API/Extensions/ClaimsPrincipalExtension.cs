using System;
using System.Security.Claims;

namespace FinanceTracking.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userIdString = user.FindFirst("sub")?.Value 
                        ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        
        if (Guid.TryParse(userIdString, out Guid userId))
        {
            return userId;
        }
        
        throw new UnauthorizedAccessException("Invalid user token.");
    }
}