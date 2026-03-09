using FinanceTracking.API.Data;
using FinanceTracking.API.Models;
using FinanceTracking.API.Services;
using FinanceTracking.API.Constants;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Moq;

namespace FinanceTracking.API.Tests.Services;

public class GroupServiceTests
{
    private FinanceDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<FinanceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new FinanceDbContext(options);
    }

    [Fact]
    public async Task UpdateGroupAsync_ShouldUpdateNameAndRecordHistory_WhenNameChanges()
    {
        var db = GetInMemoryDbContext();
        var mockHistory = new Mock<IGroupHistoryService>();
        var service = new GroupService(db, mockHistory.Object);

        var groupId = 1;
        var userId = Guid.NewGuid();
        
        var group = new Group { 
            Id = groupId, 
            Name = "Old Name",
            Members = new List<GroupMember>()
        };
        db.Groups.Add(group);
        await db.SaveChangesAsync();

        var result = await service.UpdateGroupAsync(groupId, userId, "New Shiny Name");

        result.Should().NotBeNull();
        result.Name.Should().Be("New Shiny Name");
        
        var dbGroup = await db.Groups.FindAsync(groupId);
        dbGroup!.Name.Should().Be("New Shiny Name");

        mockHistory.Verify(h => h.AddHistoryRecord(
            groupId, 
            null, 
            userId, 
            HistoryNotes.GroupRenamed, 
            null, null, null, null, 
            "Old Name", 
            "New Shiny Name"), 
        Times.Once);
    }

    [Fact]
    public async Task DeleteGroupAsync_ShouldRemoveGroup_WhenGroupExists()
    {
        var db = GetInMemoryDbContext();
        var mockHistory = new Mock<IGroupHistoryService>();
        var service = new GroupService(db, mockHistory.Object);

        var group = new Group { Id = 10, Name = "To Be Deleted" };
        db.Groups.Add(group);
        await db.SaveChangesAsync();

        await service.DeleteGroupAsync(10);

        var dbGroup = await db.Groups.FindAsync(10);
        dbGroup.Should().BeNull();
    }
}