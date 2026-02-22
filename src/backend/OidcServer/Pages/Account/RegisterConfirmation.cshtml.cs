using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OidcServer.Pages.Account;

[AllowAnonymous]
public class RegisterConfirmationModel : PageModel
{
    public IActionResult OnGet(string email, string returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return RedirectToPage("/Index");
        }

        return Page();
    }
}