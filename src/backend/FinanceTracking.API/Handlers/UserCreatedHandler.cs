using Wolverine;
using Wolverine.RabbitMQ;
using FinanceTracking.API.Data;
using FinanceTracking.API.Models;
using FinanceTracking.API.Services;
using FinanceTracking.Contracts.Events;

namespace FinanceTracking.API.Handlers;

public class UserCreatedHandler
{
    public async Task Handle(UserCreatedEvent evt, FinanceDbContext db, GroupService groupService)
    {
        var user = new AppUser
        {
            Id = evt.UserId,
            UserName = evt.Username,
            Email = evt.Email,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };
        db.Users.Add(user);
        await groupService.CreateGroupAsync(user, "Personal", true);
    }
}