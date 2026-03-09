using FinanceTracking.API.Data;
using FinanceTracking.API.Models;
using FinanceTracking.API.Services;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace FinanceTracking.API.Tests.Services;

public class GroupHistoryServiceTests
{
    private FinanceDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<FinanceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new FinanceDbContext(options);
    }

    [Fact]
    public void AddHistoryRecord_ShouldAddEntryToContext_WithoutSaving()
    {
        var db = GetInMemoryDbContext();
        var service = new GroupHistoryService(db);
        var actionUserId = Guid.NewGuid();

        service.AddHistoryRecord(1, null, actionUserId, "Test Note");

        var addedEntries = db.ChangeTracker.Entries<GroupMemberHistory>().ToList();
        
        addedEntries.Should().HaveCount(1, "because the record was staged for insertion");
        addedEntries.First().Entity.Note.Should().Be("Test Note");
        addedEntries.First().State.Should().Be(EntityState.Added);
    }

    [Fact]
    public async Task GetGroupHistoryAsync_ShouldReturnPaginatedResults()
    {
        var db = GetInMemoryDbContext();
        var service = new GroupHistoryService(db);
        
        for (int i = 0; i < 15; i++)
        {
            db.GroupMemberHistories.Add(new GroupMemberHistory 
            { 
                GroupId = 1, 
                Note = $"Note {i}", 
                ChangedAt = DateTime.UtcNow.AddMinutes(i)
            });
        }
        await db.SaveChangesAsync();

        var result = await service.GetGroupHistoryAsync(1, pageNumber: 2, pageSize: 10);

        result.TotalCount.Should().Be(15);
        result.TotalPages.Should().Be(2);
        result.CountOnPage.Should().Be(5, "because the second page of 15 items with size 10 has 5 items left");
        result.GroupHistoryEntries.Should().HaveCount(5);
    }
}