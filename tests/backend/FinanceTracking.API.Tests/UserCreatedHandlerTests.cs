using FinanceTracking.API.Data;
using FinanceTracking.API.Handlers;
using FinanceTracking.API.Models;
using FinanceTracking.API.Services;
using FinanceTracking.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Moq;

namespace FinanceTracking.API.Tests.Handlers;

public class UserCreatedHandlerTests
{
    private FinanceDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<FinanceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new FinanceDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldAddUserAndCreatePersonalGroup()
    {
        var db = GetInMemoryDbContext();
        var userId = Guid.NewGuid();
        var evt = new UserCreatedEvent(userId, "newuser@test.com", "NewUser");

        var mockGroupService = new Mock<IGroupService>();
        
        mockGroupService
            .Setup(s => s.CreateGroupAsync(It.IsAny<AppUser>(), "Personal", true))
            .ReturnsAsync(new Group())
            .Verifiable(); 

        var handler = new UserCreatedHandler();

        await handler.Handle(evt, db, mockGroupService.Object);

        var userInDb = await db.Users.FindAsync(userId);
        userInDb.Should().NotBeNull();
        userInDb!.UserName.Should().Be("NewUser");
        userInDb.Email.Should().Be("newuser@test.com");

        mockGroupService.Verify(s => s.CreateGroupAsync(
            It.Is<AppUser>(u => u.Id == userId && u.UserName == "NewUser"), 
            "Personal", 
            true), 
            Times.Once, 
            "because every new user should get a default Personal group");
    }
}