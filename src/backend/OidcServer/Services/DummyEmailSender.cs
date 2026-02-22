using Microsoft.AspNetCore.Identity.UI.Services;

namespace OidcServer.Services;

public class DummyEmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // Development only, in production should be changed to something like MailKit.
        Console.WriteLine($"\n--- EMAIL SENT --- \nTo: {email}\nSubject: {subject}\nBody: {htmlMessage}\n------------------\n");
        return Task.CompletedTask;
    }
}