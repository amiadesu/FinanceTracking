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
        var mockReceiptService = new Mock<IReceiptService>();
        
        var service = new BudgetGoalService(db, mockGroupService.Object, mockReceiptService.Object); 

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
    public async Task GetBudgetGoalProgressAsync_ShouldCalculateProgressCorrectly_UsingReceiptService()
    {
        var db = GetInMemoryDbContext();
        var mockGroupService = new Mock<IGroupService>();
        var mockReceiptService = new Mock<IReceiptService>();
        
        var service = new BudgetGoalService(db, mockGroupService.Object, mockReceiptService.Object); 

        var startDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2026, 1, 31, 23, 59, 59, DateTimeKind.Utc);
        
        var goal = new BudgetGoal 
        { 
            Id = 1,
            GroupId = 1,
            TargetAmount = 1000,
            StartDate = startDate, 
            EndDate = endDate 
        };
        db.BudgetGoals.Add(goal);
        await db.SaveChangesAsync();

        var mockedReceipts = new List<ReceiptDto>
        {
            new ReceiptDto { Id = 1, TotalAmount = 100.00m },
            new ReceiptDto { Id = 2, TotalAmount = 250.50m }
        };

        mockReceiptService
            .Setup(s => s.GetReceiptsByDateRangeAsync(1, startDate, endDate))
            .ReturnsAsync(mockedReceipts);

        var result = await service.GetBudgetGoalProgressAsync(1, 1);

        result.Should().NotBeNull();
        result.TargetAmount.Should().Be(1000);
        result.CurrentAmount.Should().Be(350.50m); // 100 + 250.50
        
        result.PercentageCompleted.Should().Be(35.05m); // (350.50 / 1000) * 100 = 35.05%
        result.AssociatedReceipts.Should().HaveCount(2);
        result.AssociatedReceipts.Should().BeEquivalentTo(mockedReceipts);
    }
}