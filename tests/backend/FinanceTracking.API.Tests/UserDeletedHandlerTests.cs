using FinanceTracking.API.Data;
using FinanceTracking.API.Handlers;
using FinanceTracking.API.Models;
using FinanceTracking.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace FinanceTracking.API.Tests.Handlers;

public class UserDeletedHandlerTests
{
    private FinanceDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<FinanceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new FinanceDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldRemoveUser_WhenUserExists()
    {
        var db = GetInMemoryDbContext();
        var userId = Guid.NewGuid();
        var existingUser = new AppUser 
        { 
            Id = userId, 
            UserName = "TestUser", 
            Email = "test@test.com" 
        };
        
        db.Users.Add(existingUser);
        await db.SaveChangesAsync();

        var handler = new UserDeletedHandler();
        var evt = new UserDeletedEvent(userId);

        await handler.Handle(evt, db);

        var userInDb = await db.Users.FindAsync(userId);
        userInDb.Should().BeNull("because the handler should have deleted the user");
    }

    [Fact]
    public async Task Handle_ShouldDoNothing_WhenUserDoesNotExist()
    {
        var db = GetInMemoryDbContext();
        var userId = Guid.NewGuid();
        
        var handler = new UserDeletedHandler();
        var evt = new UserDeletedEvent(userId);

        var act = async () => await handler.Handle(evt, db);

        await act.Should().NotThrowAsync("because deleting a non-existent user should exit gracefully");
    }
}