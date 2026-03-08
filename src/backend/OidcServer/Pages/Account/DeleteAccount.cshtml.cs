using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OidcServer.Services;

namespace OidcServer.Pages.Account;

[Authorize]
public class DeleteAccountModel : PageModel
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserEventPublisher _userEventPublisher;

    public DeleteAccountModel(
        IConfiguration configuration,
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        UserEventPublisher userEventPublisher)
    {
        _configuration = configuration;
        _userManager = userManager;
        _signInManager = signInManager;
        _userEventPublisher = userEventPublisher;
    }

    [BindProperty]
    public required InputModel Input { get; set; }

    public class InputModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = String.Empty;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!await _userManager.CheckPasswordAsync(user, Input.Password))
        {
            ModelState.AddModelError(string.Empty, "Incorrect password.");
            return Page();
        }

        // Publish the deletion event to RabbitMQ BEFORE deleting the user from the database
        await _userEventPublisher.PublishUserDeleted(user);

        var result = await _userManager.DeleteAsync(user);
        var userId = await _userManager.GetUserIdAsync(user);
        
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Unexpected error occurred deleting user with ID '{userId}'.");
        }

        await _signInManager.SignOutAsync();

        var frontendUrl = _configuration["IdentitySettings:FrontendUrl"] ?? "http://localhost:3000";
        
        return Redirect($"{frontendUrl}/?accountDeleted=true"); 
    }
}