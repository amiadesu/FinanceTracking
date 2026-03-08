using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OidcServer.Services;

namespace OidcServer.Pages.Account;

[Authorize]
public class ManageProfileModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserEventPublisher _userEventPublisher;
    private readonly IConfiguration _configuration;

    public ManageProfileModel(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        UserEventPublisher userEventPublisher,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userEventPublisher = userEventPublisher;
        _configuration = configuration;
    }

    public required string Email { get; set; }
    public required string FrontendUrl { get; set; }

    [BindProperty]
    public required InputModel Input { get; set; }

    [TempData]
    public required string StatusMessage { get; set; }

    public class InputModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; } = String.Empty;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        FrontendUrl = _configuration["IdentitySettings:FrontendUrl"] ?? "/";

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        Email = user.Email!;
        Input = new InputModel
        {
            Username = user.UserName!
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        FrontendUrl = _configuration["IdentitySettings:FrontendUrl"] ?? "/";

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var currentUsername = user.UserName;
        if (Input.Username != currentUsername)
        {
            var setUsernameResult = await _userManager.SetUserNameAsync(user, Input.Username);
            if (!setUsernameResult.Succeeded)
            {
                StatusMessage = "Unexpected error when trying to set username.";
                return RedirectToPage();
            }
            
            await _userEventPublisher.PublishUserUpdated(user);
            
            // Refresh the Identity server's local cookie so it knows about the new name
            await _signInManager.RefreshSignInAsync(user);
        }

        StatusMessage = "Your profile has been updated.";
        return RedirectToPage();
    }
}