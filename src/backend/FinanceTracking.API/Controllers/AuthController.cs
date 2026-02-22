using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceTracking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpGet("token-info")]
    [Authorize] // This forces the token validation
    public IActionResult GetTokenInfo()
    {
        Console.WriteLine("Got to api/auth/token-info");
        // Extract claims from the validated token
        var claims = User.Claims.Select(c => new 
        { 
            Type = c.Type, 
            Value = c.Value 
        });

        // Grab common identifier claims
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                     ?? User.FindFirst("sub")?.Value;

        return Ok(new
        {
            success = true,
            message = "Token is valid.",
            userId = userId,
            claims = claims
        });
    }
}