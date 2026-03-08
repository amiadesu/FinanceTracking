using FinanceTracking.API.Data;
using FinanceTracking.API.Handlers;
using FinanceTracking.API.Models;
using FinanceTracking.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace FinanceTracking.API.Tests.Handlers;

public class UserUpdatedHandlerTests
{
    private FinanceDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<FinanceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new FinanceDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldUpdateUsername_WhenUserExists()
    {
        var db = GetInMemoryDbContext();
        var userId = Guid.NewGuid();
        var originalDate = DateTime.UtcNow.AddDays(-1);
        
        var existingUser = new AppUser 
        { 
            Id = userId, 
            UserName = "OldUsername", 
            Email = "test@test.com",
            UpdatedDate = originalDate
        };
        
        db.Users.Add(existingUser);
        await db.SaveChangesAsync();

        var handler = new UserUpdatedHandler();
        var evt = new UserUpdatedEvent(userId, "NewShinyUsername");

        await handler.Handle(evt, db);

        var userInDb = await db.Users.FindAsync(userId);
        userInDb.Should().NotBeNull();
        userInDb!.UserName.Should().Be("NewShinyUsername");
        userInDb.UpdatedDate.Should().BeAfter(originalDate, "because the handler should update the timestamp");
    }
}