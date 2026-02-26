using Wolverine;
using Wolverine.RabbitMQ;
using Microsoft.AspNetCore.Identity;
using FinanceTracking.Contracts.Events;

namespace OidcServer.Services;

public class UserEventPublisher
{
    private readonly IMessageBus _bus;
    private readonly UserManager<IdentityUser> _userManager;

    public UserEventPublisher(IMessageBus bus, UserManager<IdentityUser> userManager)
    {
        _bus = bus;
        _userManager = userManager;
    }

    public async Task PublishUserCreated(IdentityUser user)
    {
        await _bus.PublishAsync(new UserCreatedEvent(
            Guid.Parse(await _userManager.GetUserIdAsync(user)),
            user.Email!,
            user.UserName!
        ));
    }

    public async Task PublishUserDeleted(IdentityUser user)
    {
        await _bus.PublishAsync(new UserDeletedEvent(
            Guid.Parse(await _userManager.GetUserIdAsync(user))
        ));
    }
}