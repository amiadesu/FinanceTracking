using FinanceTracking.API.Data;
using FinanceTracking.API.DTOs;
using FinanceTracking.API.Exceptions;
using FinanceTracking.API.Models;
using FinanceTracking.API.Services;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Moq;

namespace FinanceTracking.API.Tests.Services;

public class CategoryServiceTests
{
    private FinanceDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<FinanceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new FinanceDbContext(options);
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldThrowForbiddenException_WhenCategoryIsSystem()
    {
        var db = GetInMemoryDbContext();
        var mockGroupService = new Mock<IGroupService>();
        var service = new CategoryService(db, mockGroupService.Object);

        var systemCategory = new Category 
        { 
            Id = 1, 
            GroupId = 1, 
            Name = "Groceries", 
            ColorHex = "#FFFFFF", 
            IsSystem = true
        };
        db.Categories.Add(systemCategory);
        await db.SaveChangesAsync();

        Func<Task> act = async () => await service.DeleteCategoryAsync(1, 1);

        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage(Constants.ErrorMessages.SystemCategoryCannotBeDeleted);
    }

    [Fact]
    public async Task GetOrCreateCategoriesAsync_ShouldOnlyCreateNewCategories_WhenSomeAlreadyExist()
    {
        var db = GetInMemoryDbContext();
        var mockGroupService = new Mock<IGroupService>();
        var service = new CategoryService(db, mockGroupService.Object);

        db.Categories.Add(new Category 
        { 
            Id = 10, GroupId = 1, Name = "ExistingCategory", ColorHex = "#000000", IsSystem = false 
        });
        await db.SaveChangesAsync();

        var requestedNames = new List<string> { "ExistingCategory", "NewCategory1", "NewCategory2" };

        var result = await service.GetOrCreateCategoriesAsync(1, requestedNames);

        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        
        var totalCategoriesInDb = await db.Categories.CountAsync();
        totalCategoriesInDb.Should().Be(3, "because one already existed and two were newly created");
    }
}