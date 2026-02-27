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

    public CategoryService(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDto> CreateCategoryAsync(int groupId, CreateCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new BadRequestException(ErrorMessages.CategoryNameRequired);

        if (string.IsNullOrWhiteSpace(dto.ColorHex))
            throw new BadRequestException(ErrorMessages.CategoryColorRequired);

        if (!InputValidator.IsValidHexColor(dto.ColorHex))
            throw new BadRequestException(ErrorMessages.InvalidColorFormat);

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

    public async Task<List<CategoryDto>> GetCategoriesAsync(int groupId)
    {
        return await _context.Categories
            .Where(c => c.GroupId == groupId)
            .OrderBy(c => c.Name)
            .Select(c => Map(c))
            .ToListAsync();
    }

    public async Task<CategoryDto?> GetCategoryAsync(int groupId, int categoryId)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.GroupId == groupId && c.Id == categoryId);

        return category == null ? null : Map(category);
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
