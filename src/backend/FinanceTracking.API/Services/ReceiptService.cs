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

public class ReceiptService
{
    private readonly FinanceDbContext _context;
    private readonly GroupService _groupService;
    private readonly SellerService _sellerService;

    public ReceiptService(FinanceDbContext context, GroupService groupService, SellerService sellerService)
    {
        _context = context;
        _groupService = groupService;
        _sellerService = sellerService;
    }

    public async Task<ReceiptDto> CreateReceiptAsync(int groupId, Guid? creatorId, CreateReceiptDto dto)
    {
        if (dto.Products == null || dto.Products.Count() == 0)
            throw new BadRequestException(ErrorMessages.ReceiptShouldHaveAtLeastOneProduct);

        if (dto.Products.Count > ServiceConstants.MaxProductsPerReceipt)
            throw new BadRequestException(ErrorMessages.TooManyProductsOnReceipt);

        var now = DateTime.UtcNow;

        var receipt = new Receipt
        {
            GroupId = groupId,
            CreatedByUserId = creatorId,
            TotalAmount = 0,  // will be computed below
            PaymentDate = DateTime.SpecifyKind(dto.PaymentDate, DateTimeKind.Utc),
            CreatedDate = now,
            UpdatedDate = now,
            ProductEntries = new List<ProductEntry>()
        };

        if (dto.SellerId <= 0)
            throw new BadRequestException(ErrorMessages.SellerIdRequired);

        var existingSeller = await _sellerService.GetSellerAsync(groupId, dto.SellerId);
        if (existingSeller == null)
        {
            await _sellerService.CreateSellerAsync(
                groupId, 
                new CreateSellerDto { Id = dto.SellerId, Name = null }
            );
        }

        receipt.SellerId = dto.SellerId;

        foreach (var prod in dto.Products)
        {
            if (string.IsNullOrWhiteSpace(prod.Name))
                throw new BadRequestException(ErrorMessages.ReceiptProductNameRequired);

            var categories = await GetOrCreateCategoriesAsync(groupId, prod.Categories ?? new List<string>());
            if (categories.Count > ServiceConstants.MaxCategoriesPerProduct)
                throw new BadRequestException(ErrorMessages.TooManyProductCategories);

            var productData = await FindOrCreateProductDataAsync(groupId, prod.Name.Trim(), categories.Select(c => c.Id).ToList());

            receipt.ProductEntries.Add(new ProductEntry
            {
                GroupId = groupId,
                ProductDataId = productData.Id,
                Price = prod.Price,
                Quantity = prod.Quantity,
                CreatedDate = now,
                UpdatedDate = now
            });
        }

        var computedTotal = receipt.ProductEntries.Sum(pe => pe.Price * pe.Quantity);
        receipt.TotalAmount = computedTotal;

        _context.Receipts.Add(receipt);
        await _context.SaveChangesAsync();

        return await MapReceiptAsync(receipt.Id, receipt.GroupId);
    }

    public async Task<List<ReceiptDto>> GetReceiptsAsync(int groupId)
    {
        return await _context.Receipts
            .Where(r => r.GroupId == groupId)
            .Include(r => r.Seller)
            .Include(r => r.ProductEntries)
                .ThenInclude(pe => pe.ProductData).ThenInclude(pd => pd.ProductDataCategories)
                    .ThenInclude(pdc => pdc.Category)
            .Select(r => Map(r))
            .ToListAsync();
    }

    public async Task<List<ReceiptDto>> GetReceiptsBySellerAsync(int groupId, int sellerId)
    {
        return await _context.Receipts
            .Where(r => r.GroupId == groupId && r.SellerId == sellerId)
            .Include(r => r.Seller)
            .Include(r => r.ProductEntries)
                .ThenInclude(pe => pe.ProductData).ThenInclude(pd => pd.ProductDataCategories)
                    .ThenInclude(pdc => pdc.Category)
            .Select(r => Map(r))
            .ToListAsync();
    }

    public async Task<List<ReceiptDto>> GetReceiptsByProductDataAsync(int groupId, int productDataId)
    {
        return await _context.Receipts
            .Where(r => r.GroupId == groupId && r.ProductEntries.Any(pe => pe.ProductDataId == productDataId))
            .Include(r => r.Seller)
            .Include(r => r.ProductEntries)
                .ThenInclude(pe => pe.ProductData)
                .ThenInclude(pd => pd.ProductDataCategories)
                    .ThenInclude(pdc => pdc.Category)
            .Select(r => Map(r))
            .ToListAsync();
    }

    public async Task<ReceiptDto?> GetReceiptAsync(int groupId, int receiptId)
    {
        var r = await _context.Receipts
            .Where(x => x.GroupId == groupId && x.Id == receiptId)
            .Include(x => x.Seller)
            .Include(x => x.ProductEntries)
                .ThenInclude(pe => pe.ProductData).ThenInclude(pd => pd.ProductDataCategories)
                    .ThenInclude(pdc => pdc.Category)
            .FirstOrDefaultAsync();
        if (r == null)
            return null;

        return Map(r);
    }

    public async Task<ReceiptDto> UpdateReceiptAsync(int groupId, int receiptId, Guid userId, UpdateReceiptDto dto)
    {
        if (dto.Products == null || dto.Products.Count() == 0)
            throw new BadRequestException(ErrorMessages.ReceiptShouldHaveAtLeastOneProduct);

        if (dto.Products.Count > ServiceConstants.MaxProductsPerReceipt)
            throw new BadRequestException(ErrorMessages.TooManyProductsOnReceipt);

        var receipt = await _context.Receipts
            .Include(r => r.ProductEntries)
            .ThenInclude(pe => pe.ProductData)
            .ThenInclude(pd => pd.ProductDataCategories)
            .FirstOrDefaultAsync(r => r.GroupId == groupId && r.Id == receiptId);

        if (receipt == null)
            throw new NotFoundException(ErrorMessages.ReceiptNotFound);

        // Only author, Admin or Owner is allowed to edit the receipt
        var role = await _groupService.GetUserRoleInGroupAsync(groupId, userId);
        var isAuthor = receipt.CreatedByUserId == userId;
        var isAdminOrOwner = role.HasValue && role.Value <= GroupRole.Admin;
        if (!isAuthor && !isAdminOrOwner)
            throw new ForbiddenException(ErrorMessages.UnauthorizedAccess);

        bool changed = false;
        var now = DateTime.UtcNow;

        if (dto.SellerId.HasValue && dto.SellerId.Value <= 0)
        {
            throw new BadRequestException(ErrorMessages.SellerIdRequired);
        }

        if (dto.SellerId.HasValue && dto.SellerId != receipt.SellerId)
        {
            var existingSeller = await _sellerService.GetSellerAsync(groupId, dto.SellerId.Value);
            if (existingSeller == null)
            {
                await _sellerService.CreateSellerAsync(
                    groupId, 
                    new CreateSellerDto { Id = dto.SellerId.Value, Name = null }
                );
            }

            receipt.SellerId = dto.SellerId.Value;
            changed = true;
        }
        if (dto.PaymentDate.HasValue && dto.PaymentDate.Value != receipt.PaymentDate)
        {
            receipt.PaymentDate = DateTime.SpecifyKind(dto.PaymentDate.Value, DateTimeKind.Utc);
            changed = true;
        }

        var incomingIds = dto.Products.Where(p => p.Id.HasValue).Select(p => p.Id.Value).ToList();
        var entriesToRemove = receipt.ProductEntries.Where(pe => !incomingIds.Contains(pe.Id)).ToList();

        foreach (var entry in entriesToRemove)
        {
            _context.ProductEntries.Remove(entry);
            changed = true;
        }

        foreach (var prod in dto.Products)
        {
            if (string.IsNullOrWhiteSpace(prod.Name))
                throw new BadRequestException(ErrorMessages.ReceiptProductNameRequired);

            if (!prod.Price.HasValue)
                throw new BadRequestException(ErrorMessages.ReceiptProductPriceRequired);
            if (!prod.Quantity.HasValue)
                throw new BadRequestException(ErrorMessages.ReceiptProductQuantityRequired);

            var categories = await GetOrCreateCategoriesAsync(groupId, prod.Categories ?? new List<string>());
            if (categories.Count > ServiceConstants.MaxCategoriesPerProduct)
                throw new BadRequestException(ErrorMessages.TooManyProductCategories);

            var productData = await FindOrCreateProductDataAsync(groupId, prod.Name.Trim(), categories.Select(c => c.Id).ToList());

            var existingEntry = prod.Id.HasValue 
                ? receipt.ProductEntries.FirstOrDefault(pe => pe.Id == prod.Id.Value) 
                : null;

            if (existingEntry != null)
            {
                if (existingEntry.Price != prod.Price.Value || 
                    existingEntry.Quantity != prod.Quantity.Value || 
                    existingEntry.ProductDataId != productData.Id)
                {
                    existingEntry.Price = prod.Price.Value;
                    existingEntry.Quantity = prod.Quantity.Value;
                    existingEntry.ProductDataId = productData.Id;
                    existingEntry.UpdatedDate = now;
                    changed = true;
                }
            }
            else
            {
                receipt.ProductEntries.Add(new ProductEntry
                {
                    GroupId = groupId,
                    ProductDataId = productData.Id,
                    Price = prod.Price.Value,
                    Quantity = prod.Quantity.Value,
                    CreatedDate = now,
                    UpdatedDate = now
                });
                changed = true;
            }
        }

        var computedTotal = receipt.ProductEntries.Sum(pe => pe.Price * pe.Quantity);
        if (receipt.TotalAmount != computedTotal)
        {
            receipt.TotalAmount = computedTotal;
            changed = true;
        }

        if (changed)
        {
            receipt.UpdatedDate = now;
            await _context.SaveChangesAsync();
        }

        return Map(receipt);
    }

    public async Task DeleteReceiptAsync(int groupId, int receiptId, Guid userId)
    {
        var receipt = await _context.Receipts
            .Include(r => r.ProductEntries)
            .FirstOrDefaultAsync(r => r.GroupId == groupId && r.Id == receiptId);

        if (receipt == null)
            throw new NotFoundException(ErrorMessages.ReceiptNotFound);

        var role = await _groupService.GetUserRoleInGroupAsync(groupId, userId);
        var isAuthor = receipt.CreatedByUserId == userId;
        var isAdminOrOwner = role.HasValue && role.Value <= GroupRole.Admin;
        if (!isAuthor && !isAdminOrOwner)
            throw new ForbiddenException(ErrorMessages.UnauthorizedAccess);

        var dataIdsToCheck = receipt.ProductEntries
            .Select(pe => pe.ProductDataId)
            .Distinct()
            .ToList();

        _context.Receipts.Remove(receipt);
        // Saving here for cascading delete of ProductEntries, 
        // so that we can check for orphaned ProductData afterwards
        await _context.SaveChangesAsync();

        foreach (var dataId in dataIdsToCheck)
        {
            bool hasOtherEntries = await _context.ProductEntries
                .AnyAsync(pe => pe.ProductDataId == dataId && pe.GroupId == groupId);

            if (!hasOtherEntries)
            {
                var orphanedData = await _context.ProductData
                    .FirstOrDefaultAsync(pd => pd.Id == dataId && pd.GroupId == groupId);

                if (orphanedData != null)
                {
                    _context.ProductData.Remove(orphanedData);
                }
            }
        }
        
        await _context.SaveChangesAsync();
    }

    private static ReceiptDto Map(Receipt r)
    {
        var products = r.ProductEntries?.Select(pe => new ReceiptProductDto
        {
            Id = pe.Id,
            Name = pe.ProductData?.Name,
            Categories = pe.ProductData?.ProductDataCategories?
                .Select(pdc => pdc.Category.Name)
                .ToList() ?? new List<string>(),
            Price = pe.Price,
            Quantity = pe.Quantity
        }).ToList() ?? new List<ReceiptProductDto>();

        return new ReceiptDto
        {
            Id = r.Id,
            GroupId = r.GroupId,
            CreatedByUserId = r.CreatedByUserId,
            SellerId = r.SellerId,
            SellerName = r.Seller?.Name,
            TotalAmount = r.TotalAmount,
            PaymentDate = r.PaymentDate,
            CreatedDate = r.CreatedDate,
            UpdatedDate = r.UpdatedDate,
            Products = products
        };
    }

    private async Task<ReceiptDto> MapReceiptAsync(int receiptId, int groupId)
    {
        var r = await _context.Receipts
            .Include(x => x.Seller)
            .Include(x => x.ProductEntries)
                .ThenInclude(pe => pe.ProductData).ThenInclude(pd => pd.ProductDataCategories)
                    .ThenInclude(pdc => pdc.Category)
            .FirstOrDefaultAsync(x => x.GroupId == groupId && x.Id == receiptId);

        if (r == null)
            throw new NotFoundException(ErrorMessages.ReceiptNotFound);

        return Map(r);
    }

    private async Task<List<Category>> GetOrCreateCategoriesAsync(int groupId, List<string> names)
    {
        var processed = names
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (!processed.Any())
            return new List<Category>();

        var existing = await _context.Categories
            .Where(c => c.GroupId == groupId && processed.Contains(c.Name))
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

    private async Task<ProductData> FindOrCreateProductDataAsync(int groupId, string name, List<int> categoryIds)
    {
        var candidates = await _context.ProductData
            .Include(pd => pd.ProductDataCategories)
            .Where(pd => pd.GroupId == groupId && pd.Name == name)
            .ToListAsync();

        categoryIds.Sort();

        foreach (var pd in candidates)
        {
            var ids = pd.ProductDataCategories.Select(pdc => pdc.CategoryId).OrderBy(x => x).ToList();
            if (ids.SequenceEqual(categoryIds))
                return pd;
        }

        var now = DateTime.UtcNow;
        var product = new ProductData
        {
            GroupId = groupId,
            Name = name,
            Description = null,
            CreatedDate = now,
            UpdatedDate = now,
            ProductDataCategories = new List<ProductDataCategory>()
        };

        foreach (var catId in categoryIds.Distinct())
        {
            product.ProductDataCategories.Add(new ProductDataCategory
            {
                CategoryId = catId,
                GroupId = groupId
            });
        }

        _context.ProductData.Add(product);
        await _context.SaveChangesAsync();

        return product;
    }
}