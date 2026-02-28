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

namespace FinanceTracking.API.Services;

public class ProductDataService
{
    private readonly FinanceDbContext _context;

    public ProductDataService(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductDataDto>> GetProductsAsync(int groupId)
    {
        return await _context.ProductData
            .Where(p => p.GroupId == groupId)
            .Include(p => p.ProductDataCategories)
                .ThenInclude(pdc => pdc.Category)
            .OrderBy(p => p.Name)
            .Select(p => Map(p))
            .ToListAsync();
    }

    public async Task<ProductDataDto?> GetProductAsync(int groupId, int productId)
    {
        var product = await _context.ProductData
            .Include(p => p.ProductDataCategories)
                .ThenInclude(pdc => pdc.Category)
            .FirstOrDefaultAsync(p => p.GroupId == groupId && p.Id == productId);

        return product == null ? null : Map(product);
    }

    public async Task<ProductDataDto?> UpdateProductAsync(int groupId, int productId, UpdateProductDataDto dto)
    {
        var product = await _context.ProductData
            .Include(p => p.ProductDataCategories)
            .FirstOrDefaultAsync(p => p.GroupId == groupId && p.Id == productId);

        if (product == null) return null;

        bool changed = false;

        if (dto.Name != null && !string.IsNullOrWhiteSpace(dto.Name))
        {
            product.Name = dto.Name.Trim();
            changed = true;
        }

        if (dto.Description != null)
        {
            product.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
            changed = true;
        }

        if (dto.CategoryIds != null)
        {
            if (dto.CategoryIds.Count > ServiceConstants.MaxCategoriesPerProduct)
                throw new BadRequestException(ErrorMessages.TooManyProductCategories);

            var incomingIds = dto.CategoryIds.Distinct().ToList();
            var currentIds = product.ProductDataCategories.Select(c => c.CategoryId).ToList();

            if (!incomingIds.OrderBy(id => id).SequenceEqual(currentIds.OrderBy(id => id)))
            {
                _context.ProductDataCategories.RemoveRange(product.ProductDataCategories);
                
                foreach (var catId in incomingIds)
                {
                    product.ProductDataCategories.Add(new ProductDataCategory
                    {
                        ProductDataId = productId,
                        GroupId = groupId,
                        CategoryId = catId
                    });
                }
                changed = true;
            }
        }

        if (changed)
        {
            product.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            // Reload with category details for the return mapping
            await _context.Entry(product)
                .Collection(p => p.ProductDataCategories)
                .Query()
                .Include(pdc => pdc.Category)
                .LoadAsync();
        }

        return Map(product);
    }

    public async Task<bool> DeleteProductAsync(int groupId, int productId)
    {
        var product = await _context.ProductData
            .Include(p => p.ProductDataCategories)
            .FirstOrDefaultAsync(p => p.GroupId == groupId && p.Id == productId);

        if (product == null) return false;

        // Check if the product is used in any receipts
        var isUsed = await _context.ProductEntries
            .AnyAsync(pe => pe.GroupId == groupId && pe.ProductDataId == productId);

        if (isUsed)
        {
            throw new InvalidOperationException(ErrorMessages.ProductNotOrphaned);
        }

        // Remove category mappings before deleting the product
        if (product.ProductDataCategories.Any())
        {
            _context.ProductDataCategories.RemoveRange(product.ProductDataCategories);
        }

        _context.ProductData.Remove(product);
        await _context.SaveChangesAsync();

        return true;
    }

    private static ProductDataDto Map(ProductData p) => new ProductDataDto
    {
        Id = p.Id,
        GroupId = p.GroupId,
        Name = p.Name,
        Description = p.Description,
        Categories = p.ProductDataCategories?.Select(pdc => pdc.Category.Name).ToList() ?? new List<string>(),
        CreatedDate = p.CreatedDate,
        UpdatedDate = p.UpdatedDate
    };
}