using Wolverine;
using Wolverine.RabbitMQ;
using FinanceTracking.API.Data;
using FinanceTracking.API.Models;
using FinanceTracking.API.Services;
using FinanceTracking.Contracts.Events;

namespace FinanceTracking.API.Handlers;

public class UserDeletedHandler
{
    public async Task Handle(UserDeletedEvent evt, FinanceDbContext db)
    {
        var user = await db.Users.FindAsync(evt.UserId);
        if (user is not null)
        {
            db.Users.Remove(user);
            await db.SaveChangesAsync();
        }
    }
}