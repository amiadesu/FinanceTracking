using FinanceTracking.API.Data;
using FinanceTracking.Contracts.Events;

namespace FinanceTracking.API.Handlers;

public class UserUpdatedHandler
{
    public async Task Handle(UserUpdatedEvent evt, FinanceDbContext db)
    {
        var user = await db.Users.FindAsync(evt.UserId);
        
        if (user is not null)
        {
            user.UserName = evt.NewUsername;
            user.UpdatedDate = DateTime.UtcNow; 
            
            await db.SaveChangesAsync();
        }
    }
}