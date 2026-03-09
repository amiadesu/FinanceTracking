using FinanceTracking.API.Data;
using FinanceTracking.API.Models;
using FinanceTracking.API.Services;
using FinanceTracking.API.Constants;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace FinanceTracking.API.Tests.Services;

public class ProductDataServiceTests
{
    private FinanceDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<FinanceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new FinanceDbContext(options);
    }

    [Fact]
    public async Task FindOrCreateProductDataAsync_ShouldReturnExisting_WhenExactCategoryMatchExists()
    {
        var db = GetInMemoryDbContext();
        var service = new ProductDataService(db);

        var existingProduct = new ProductData 
        { 
            Id = 50, 
            GroupId = 1, 
            Name = "Milk",
            ProductDataCategories = new List<ProductDataCategory> 
            {
                new ProductDataCategory { CategoryId = 1, GroupId = 1 },
                new ProductDataCategory { CategoryId = 2, GroupId = 1 }
            }
        };
        db.ProductData.Add(existingProduct);
        await db.SaveChangesAsync();

        var result = await service.FindOrCreateProductDataAsync(1, "Milk", new List<int> { 2, 1 });

        result.Id.Should().Be(50, "because it should have found and reused the existing product");
        var totalProducts = await db.ProductData.CountAsync();
        totalProducts.Should().Be(1, "because a new product should not have been created");
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldThrowInvalidOperationException_WhenProductIsUsedInReceipts()
    {
        var db = GetInMemoryDbContext();
        var service = new ProductDataService(db);

        db.ProductData.Add(new ProductData { Id = 10, GroupId = 1, Name = "Apple" });
        db.ProductEntries.Add(new ProductEntry { Id = 1, GroupId = 1, ProductDataId = 10 });
        await db.SaveChangesAsync();

        Func<Task> act = async () => await service.DeleteProductAsync(1, 10);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage(ErrorMessages.ProductNotOrphaned);
    }
}