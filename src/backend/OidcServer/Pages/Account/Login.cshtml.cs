using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OidcServer.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public LoginModel(
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [BindProperty]
    [Display(Name = "Email or Username")]
    public string Username { get; set; }

    [BindProperty]
    public string Password { get; set; }

    [BindProperty(SupportsGet = true)]
    public string ReturnUrl { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            var userNameToLogInOut = Username;

            // Check if the input looks like an email
            if (new EmailAddressAttribute().IsValid(Username))
            {
                var user = await _userManager.FindByEmailAsync(Username);
                if (user != null)
                {
                    userNameToLogInOut = user.UserName;
                }
            }

            var result = await _signInManager.PasswordSignInAsync(userNameToLogInOut, Password, false, lockoutOnFailure: false);
            
            if (result.Succeeded)
            {
                return LocalRedirect(ReturnUrl ?? "/");
            }
            if (result.IsNotAllowed)
            {
                ModelState.AddModelError(string.Empty, "You must confirm your email before logging in.");
                return Page();
            }
            
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }
        return Page();
    }
}