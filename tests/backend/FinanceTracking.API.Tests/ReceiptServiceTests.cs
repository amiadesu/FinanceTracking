using FinanceTracking.API.Data;
using FinanceTracking.API.DTOs;
using FinanceTracking.API.Exceptions;
using FinanceTracking.API.Models;
using FinanceTracking.API.Services;
using FinanceTracking.API.Constants;
using FinanceTracking.API.Utils;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Moq;
using Wolverine;

namespace FinanceTracking.API.Tests.Services;

public class ReceiptServiceTests
{
    private FinanceDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<FinanceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new FinanceDbContext(options);
    }

    [Fact]
    public async Task CreateReceiptAsync_ShouldThrowBadRequest_WhenProductListIsEmpty()
    {
        var db = GetInMemoryDbContext();
        
        var mockGroupService = new Mock<IGroupService>();
        var mockGroupMemberService = new Mock<IGroupMemberService>();
        var mockSellerService = new Mock<ISellerService>();
        var mockCategoryService = new Mock<ICategoryService>();
        var mockProductDataService = new Mock<IProductDataService>();
        var mockMessageBus = new Mock<IMessageBus>();
        var mockPendingPredictions = new Mock<IPendingPredictionRequests>();

        var service = new ReceiptService(db, mockGroupService.Object, mockGroupMemberService.Object, 
            mockSellerService.Object, mockCategoryService.Object, mockProductDataService.Object, 
            mockMessageBus.Object, mockPendingPredictions.Object);

        var dto = new CreateReceiptDto 
        { 
            SellerId = "123", 
            PaymentDate = DateTime.UtcNow, 
            Products = new List<CreateReceiptProductDto>()
        };

        Func<Task> act = async () => await service.CreateReceiptAsync(1, Guid.NewGuid(), dto);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage(ErrorMessages.ReceiptShouldHaveAtLeastOneProduct);
    }

    [Fact]
    public async Task DeleteReceiptAsync_ShouldDeleteReceiptAndCleanUpOrphanedProductData()
    {
        var db = GetInMemoryDbContext();
        
        var mockGroupService = new Mock<IGroupService>();
        var mockGroupMemberService = new Mock<IGroupMemberService>();
        var mockSellerService = new Mock<ISellerService>();
        var mockCategoryService = new Mock<ICategoryService>();
        var mockProductDataService = new Mock<IProductDataService>();
        var mockMessageBus = new Mock<IMessageBus>();
        var mockPendingPredictions = new Mock<IPendingPredictionRequests>();

        var service = new ReceiptService(db, mockGroupService.Object, mockGroupMemberService.Object, 
            mockSellerService.Object, mockCategoryService.Object, mockProductDataService.Object, 
            mockMessageBus.Object, mockPendingPredictions.Object);

        var authorId = Guid.NewGuid();
        
        db.ProductData.Add(new ProductData { Id = 100, GroupId = 1, Name = "TestItem" });
        db.Receipts.Add(new Receipt 
        { 
            Id = 5, 
            GroupId = 1, 
            CreatedByUserId = authorId,
            SellerId = "123",
            ProductEntries = new List<ProductEntry> 
            {
                new ProductEntry { Id = 1, GroupId = 1, ProductDataId = 100 }
            }
        });
        await db.SaveChangesAsync();

        await service.DeleteReceiptAsync(1, 5, authorId);

        var receiptInDb = await db.Receipts
            .FirstOrDefaultAsync(r => r.GroupId == 1 && r.Id == 5);
        receiptInDb.Should().BeNull("because the receipt was deleted");

        var productDataInDb = await db.ProductData
            .FirstOrDefaultAsync(pd => pd.GroupId == 1 && pd.Id == 100);
        productDataInDb.Should().BeNull("because it was orphaned and should be cleaned up by the service");
    }
}