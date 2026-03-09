using FinanceTracking.API.Data;
using FinanceTracking.API.DTOs;
using FinanceTracking.API.Models;
using FinanceTracking.API.Services;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace FinanceTracking.API.Tests.Services;

public class StatisticsServiceTests
{
    private FinanceDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<FinanceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new FinanceDbContext(options);
    }

    [Fact]
    public async Task GetTopProductsAsync_ShouldFilterByDateAndReturnTopProductsOrderedBySpent()
    {
        // Arrange
        var db = GetInMemoryDbContext();
        var service = new StatisticsService(db);
        var groupId = 1;
        var userId = Guid.NewGuid();

        var apple = new ProductData { Id = 1, GroupId = groupId, Name = "Apple" };
        var banana = new ProductData { Id = 2, GroupId = groupId, Name = "Banana" };
        db.ProductData.AddRange(apple, banana);

        var validDate = new DateTime(2026, 3, 10, 12, 0, 0, DateTimeKind.Utc);
        var invalidDate = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        var validReceipt = new Receipt { 
            Id = 1, 
            GroupId = groupId,
            SellerId = "123", 
            PaymentDate = validDate, 
            CreatedByUserId = userId 
        };
        var invalidReceipt = new Receipt { 
            Id = 2, 
            GroupId = groupId, 
            SellerId = "456",
            PaymentDate = invalidDate, 
            CreatedByUserId = userId 
        };
        db.Receipts.AddRange(validReceipt, invalidReceipt);

        db.ProductEntries.AddRange(
            // Valid Apple: 5 qty * 2.00 = 10.00 spent
            new ProductEntry { Id = 1, GroupId = groupId, ReceiptId = 1, ProductDataId = 1, Quantity = 5, Price = 2.00m },
            // Valid Banana: 2 qty * 10.00 = 20.00 spent (Should be #1)
            new ProductEntry { Id = 2, GroupId = groupId, ReceiptId = 1, ProductDataId = 2, Quantity = 2, Price = 10.00m },
            // Invalid Apple (Outside date range): Should be ignored
            new ProductEntry { Id = 3, GroupId = groupId, ReceiptId = 2, ProductDataId = 1, Quantity = 100, Price = 5.00m }
        );
        await db.SaveChangesAsync();

        var filter = new StatisticsFilterDto
        {
            StartDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 3, 31, 23, 59, 59, DateTimeKind.Utc),
            Top = 5
        };

        var result = await service.GetTopProductsAsync(groupId, userId, filter);

        result.Should().NotBeNull();
        result.Should().HaveCount(2, "because the third entry was out of the date range");

        // Banana should be first because it had the highest TotalSpent (20.00 vs 10.00)
        result[0].ProductName.Should().Be("Banana");
        result[0].TotalQuantity.Should().Be(2);
        result[0].TotalSpent.Should().Be(20.00m);

        result[1].ProductName.Should().Be("Apple");
        result[1].TotalQuantity.Should().Be(5);
        result[1].TotalSpent.Should().Be(10.00m);
    }

    [Fact]
    public async Task GetSpendingHistoryAsync_ShouldFilterByPersonalBudgetAndGroupDatesCorrectly()
    {
        var db = GetInMemoryDbContext();
        var service = new StatisticsService(db);
        var groupId = 1;
        
        var targetUserId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        db.ProductData.Add(new ProductData { Id = 1, GroupId = groupId, Name = "Item" });

        var date1 = new DateTime(2026, 3, 10, 12, 0, 0, DateTimeKind.Utc);
        var date2 = new DateTime(2026, 3, 15, 12, 0, 0, DateTimeKind.Utc);

        // Target user's receipts
        db.Receipts.Add(new Receipt { 
            Id = 1, 
            GroupId = groupId, 
            SellerId = "123",
            PaymentDate = date1, 
            CreatedByUserId = targetUserId
        });
        db.Receipts.Add(new Receipt { 
            Id = 2, 
            GroupId = groupId, 
            SellerId = "456",
            PaymentDate = date2, 
            CreatedByUserId = targetUserId
        });
        
        // Other user's receipt
        db.Receipts.Add(new Receipt { 
            Id = 3, 
            GroupId = groupId, 
            SellerId = "789",
            PaymentDate = date1, 
            CreatedByUserId = otherUserId 
        });

        db.ProductEntries.AddRange(
            new ProductEntry { Id = 1, GroupId = groupId, ReceiptId = 1, ProductDataId = 1, Quantity = 1, Price = 50.00m },
            new ProductEntry { Id = 2, GroupId = groupId, ReceiptId = 2, ProductDataId = 1, Quantity = 2, Price = 20.00m },
            new ProductEntry { Id = 3, GroupId = groupId, ReceiptId = 3, ProductDataId = 1, Quantity = 1, Price = 1000.00m }
        );
        await db.SaveChangesAsync();

        var filter = new StatisticsFilterDto
        {
            StartDate = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 3, 31, 23, 59, 59, DateTimeKind.Utc),
            IsPersonalBudgetOnly = true
        };

        var result = await service.GetSpendingHistoryAsync(groupId, targetUserId, filter);

        result.Should().NotBeNull();
        result.Should().HaveCount(2, "because there are two distinct dates for the target user");

        // Dates should be sorted ascending
        result[0].Date.Should().Be(date1.Date);
        result[0].TotalSpent.Should().Be(50.00m, "because the 1000.00 spent by the other user should be excluded");

        result[1].Date.Should().Be(date2.Date);
        result[1].TotalSpent.Should().Be(40.00m);
    }
}