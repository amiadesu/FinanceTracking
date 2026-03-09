using FinanceTracking.API.Data;
using FinanceTracking.API.DTOs;
using FinanceTracking.API.Models;
using FinanceTracking.API.Services;
using FinanceTracking.API.Constants;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Moq;

namespace FinanceTracking.API.Tests.Services;

public class SellerServiceTests
{
    private FinanceDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<FinanceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new FinanceDbContext(options);
    }

    [Fact]
    public async Task CreateSellerAsync_ShouldSaveSeller_WhenIdIsNumericString()
    {
        var db = GetInMemoryDbContext();
        var mockGroupService = new Mock<IGroupService>();
        var service = new SellerService(db, mockGroupService.Object);

        var dto = new CreateSellerDto { Id = "12345678", Name = "Local Supermarket", Description = "Groceries" };

        var result = await service.CreateSellerAsync(1, dto);

        result.Should().NotBeNull();
        result.Id.Should().Be("12345678");
        result.Name.Should().Be("Local Supermarket");

        var sellerInDb = await db.Sellers.FirstOrDefaultAsync(s => s.Id == "12345678" && s.GroupId == 1);
        sellerInDb.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteSellerAsync_ShouldThrowInvalidOperationException_WhenSellerIsUsedInReceipts()
    {
        var db = GetInMemoryDbContext();
        var mockGroupService = new Mock<IGroupService>();
        var service = new SellerService(db, mockGroupService.Object);

        var sellerId = "99999";
        db.Sellers.Add(new Seller { Id = sellerId, GroupId = 1, Name = "Used Seller" });
        db.Receipts.Add(new Receipt { Id = 1, GroupId = 1, SellerId = sellerId });
        await db.SaveChangesAsync();

        Func<Task> act = async () => await service.DeleteSellerAsync(1, sellerId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage(ErrorMessages.SellerNotOrphaned);
    }
}