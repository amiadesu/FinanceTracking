using FinanceTracking.API.Data;
using FinanceTracking.API.DTOs;
using FinanceTracking.API.Exceptions;
using FinanceTracking.API.Models;
using FinanceTracking.API.Services;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Moq;

namespace FinanceTracking.API.Tests.Services;

public class BudgetGoalServiceTests
{
    private FinanceDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<FinanceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new FinanceDbContext(options);
    }

    [Fact]
    public async Task CreateBudgetGoalAsync_ShouldThrowBadRequest_WhenEndDateIsBeforeStartDate()
    {
        var db = GetInMemoryDbContext();
        var mockGroupService = new Mock<IGroupService>();
        var service = new BudgetGoalService(db, mockGroupService.Object);

        var dto = new CreateBudgetGoalDto 
        { 
            TargetAmount = 1000,
            StartDate = DateTime.UtcNow, 
            EndDate = DateTime.UtcNow.AddDays(-1) // Invalid: End is before Start
        };

        Func<Task> act = async () => await service.CreateBudgetGoalAsync(1, dto);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task GetBudgetGoalProgressAsync_ShouldCalculateSumCorrectly_IgnoringOutdatedReceipts()
    {
        var db = GetInMemoryDbContext();
        var mockGroupService = new Mock<IGroupService>();
        var service = new BudgetGoalService(db, mockGroupService.Object);

        var goal = new BudgetGoal 
        { 
            Id = 1, 
            GroupId = 1, 
            TargetAmount = 1000, 
            StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), 
            EndDate = new DateTime(2026, 1, 31, 23, 59, 59, DateTimeKind.Utc) 
        };
        db.BudgetGoals.Add(goal);
        
        db.Receipts.AddRange(
            // Inside the date range
            new Receipt { 
                Id = 1, 
                GroupId = 1,
                SellerId = "123",
                TotalAmount = 100.00m, 
                PaymentDate = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc) 
            },
            new Receipt { 
                Id = 2, 
                GroupId = 1, 
                SellerId = "456",
                TotalAmount = 250.50m, 
                PaymentDate = new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc) 
            },
            
            // Outside the date range (should be ignored in sum)
            new Receipt { 
                Id = 3, 
                GroupId = 1, 
                SellerId = "789",
                TotalAmount = 500.00m, 
                PaymentDate = new DateTime(2026, 2, 5, 0, 0, 0, DateTimeKind.Utc) 
            } 
        );
        await db.SaveChangesAsync();

        var result = await service.GetBudgetGoalProgressAsync(1, 1);

        result.Should().NotBeNull();
        result.CurrentAmount.Should().Be(350.50m); // 100 + 250.50
        result.TargetAmount.Should().Be(1000);
    }
}