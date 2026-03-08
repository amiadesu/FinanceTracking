using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FinanceTracking.API.Data;
using FinanceTracking.API.DTOs;
using FinanceTracking.API.Models;
using FinanceTracking.API.Exceptions;
using FinanceTracking.API.Constants;
using FinanceTracking.API.Validators;

namespace FinanceTracking.API.Services;

public class CategoryService
{
    private readonly FinanceDbContext _context;
    private readonly IGroupService _groupService;

    public CategoryService(
        FinanceDbContext context,
        IGroupService groupService)
    {
        _context = context;
        _groupService = groupService;
    }

    public async Task<CategoryDto> CreateCategoryAsync(int groupId, CreateCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new BadRequestException(ErrorMessages.CategoryNameRequired);

        if (string.IsNullOrWhiteSpace(dto.ColorHex))
            throw new BadRequestException(ErrorMessages.CategoryColorRequired);

        if (!InputValidator.IsValidHexColor(dto.ColorHex))
            throw new BadRequestException(ErrorMessages.InvalidColorFormat);

        var currentCategoryCount = await _context.Categories
            .CountAsync(c => c.GroupId == groupId && !c.IsSystem);

        if (currentCategoryCount >= ServiceConstants.MaxCategoriesPerGroup)
            throw new BadRequestException(ErrorMessages.TooManyCategoriesInGroup);

        var now = DateTime.UtcNow;

        var category = new Category
        {
            GroupId = groupId,
            Name = dto.Name.Trim(),
            ColorHex = dto.ColorHex.Trim(),
            IsSystem = false,
            CreatedDate = now,
            UpdatedDate = now
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return Map(category);
    }

    public async Task<CategoryListResponseDto> GetCategoriesAsync(int groupId)
    {
        var categories = await _context.Categories
            .Where(c => c.GroupId == groupId)
            .OrderBy(c => c.Name)
            .Select(c => Map(c))
            .ToListAsync();
        return new CategoryListResponseDto
        {
            CurrentCount = categories.Count,
            MaxAllowed = await _groupService.GetGroupMaxCategoriesAsync(groupId),
            Categories = categories
        };
    }

    public async Task<CategoryDto?> GetCategoryAsync(int groupId, int categoryId)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == categoryId && (c.GroupId == groupId || c.IsSystem));

        return category == null ? null : Map(category);
    }

    public async Task<List<CategoryDto>> GetSystemCategoriesAsync()
    {
        return await _context.Categories
            .Where(c => c.IsSystem)
            .OrderBy(c => c.Name)
            .Select(c => Map(c))
            .ToListAsync();
    }

    public async Task<List<Category>> GetOrCreateCategoriesAsync(int groupId, List<string> names)
    {
        var processed = names
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (!processed.Any())
            return new List<Category>();

        var existing = await _context.Categories
            .Where(c => (c.GroupId == groupId || c.IsSystem) && processed.Contains(c.Name))
            .ToListAsync();

        var existingNames = existing.Select(c => c.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var toAdd = processed
            .Where(n => !existingNames.Contains(n))
            .Select(n => new Category
            {
                GroupId = groupId,
                Name = n,
                ColorHex = ServiceConstants.DefaultCategoryColor,
                IsSystem = false,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            })
            .ToList();

        if (toAdd.Any())
        {
            _context.Categories.AddRange(toAdd);
            await _context.SaveChangesAsync();
            existing.AddRange(toAdd);
        }

        return existing;
    }

    public async Task<CategoryDto> UpdateCategoryAsync(int groupId, int categoryId, UpdateCategoryDto dto)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.GroupId == groupId && c.Id == categoryId);

        if (category == null)
            throw new NotFoundException(ErrorMessages.CategoryNotFound);

        if (category.IsSystem)
            throw new ForbiddenException(ErrorMessages.SystemCategoryCannotBeModified);

        bool changed = false;

        if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name != category.Name)
        {
            category.Name = dto.Name.Trim();
            changed = true;
        }

        if (!string.IsNullOrWhiteSpace(dto.ColorHex) && dto.ColorHex != category.ColorHex)
        {
            if (!InputValidator.IsValidHexColor(dto.ColorHex))
                throw new BadRequestException(ErrorMessages.InvalidColorFormat);

            category.ColorHex = dto.ColorHex.Trim();
            changed = true;
        }

        if (changed)
        {
            category.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return Map(category);
    }

    public async Task DeleteCategoryAsync(int groupId, int categoryId)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.GroupId == groupId && c.Id == categoryId);

        if (category == null)
            throw new NotFoundException(ErrorMessages.CategoryNotFound);

        if (category.IsSystem)
            throw new ForbiddenException(ErrorMessages.SystemCategoryCannotBeDeleted);

        var productDataCategories = await _context.ProductDataCategories
            .Where(pdc => pdc.CategoryId == categoryId && pdc.GroupId == groupId)
            .ToListAsync();

        _context.ProductDataCategories.RemoveRange(productDataCategories);

        _context.Categories.Remove(category);

        await _context.SaveChangesAsync();
    }

    private static CategoryDto Map(Category category) => new CategoryDto
    {
        Id = category.Id,
        GroupId = category.GroupId,
        Name = category.Name,
        ColorHex = category.ColorHex,
        IsSystem = category.IsSystem,
        CreatedDate = category.CreatedDate,
        UpdatedDate = category.UpdatedDate
    };
}