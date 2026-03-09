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
using FinanceTracking.API.Parsers;
using FinanceTracking.API.Validators;
using FinanceTracking.API.Utils;
using FinanceTracking.API.Constants;
using Wolverine;

namespace FinanceTracking.API.Services;

public class ReceiptService: IReceiptService
{
    private readonly FinanceDbContext _context;
    private readonly IGroupService _groupService;
    private readonly IGroupMemberService _groupMemberService;
    private readonly SellerService _sellerService;
    private readonly ICategoryService _categoryService;
    private readonly IProductDataService _productDataService;
    private readonly IMessageBus _messageBus;
    private readonly IPendingPredictionRequests _pendingPredictions;

    public ReceiptService(
        FinanceDbContext context,
        IGroupService groupService,
        IGroupMemberService groupMemberService, 
        SellerService sellerService,
        ICategoryService categoryService,
        IProductDataService productDataService,
        IMessageBus messageBus,
        IPendingPredictionRequests pendingPredictions)
    {
        _context = context;
        _groupService = groupService;
        _groupMemberService = groupMemberService;
        _sellerService = sellerService;
        _categoryService = categoryService;
        _productDataService = productDataService;
        _messageBus = messageBus;
        _pendingPredictions = pendingPredictions;
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

        if (string.IsNullOrWhiteSpace(dto.SellerId))
            throw new BadRequestException(ErrorMessages.SellerIdRequired);

        if (!InputValidator.IsNumericString(dto.SellerId))
            throw new BadRequestException(ErrorMessages.InvalidSellerId);

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

            var categories = await _categoryService.GetOrCreateCategoriesAsync(groupId, prod.Categories ?? new List<string>());
            if (categories.Count > ServiceConstants.MaxCategoriesPerProduct)
                throw new BadRequestException(ErrorMessages.TooManyProductCategories);

            var productData = await _productDataService.FindOrCreateProductDataAsync(groupId, prod.Name.Trim(), categories.Select(c => c.Id).ToList());

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

        var computedTotal = FinancialCalculator.CalculateReceiptTotal(receipt.ProductEntries);
        receipt.TotalAmount = computedTotal;

        _context.Receipts.Add(receipt);
        await _context.SaveChangesAsync();

        return await MapReceiptAsync(receipt.Id, receipt.GroupId);
    }

    public async Task<ReceiptListResponseDto> GetReceiptsAsync(int groupId)
    {
        var receipts = await _context.Receipts
            .Where(r => r.GroupId == groupId)
            .Include(r => r.CreatedByUser)
            .Include(r => r.Seller)
            .Include(r => r.ProductEntries)
                .ThenInclude(pe => pe.ProductData).ThenInclude(pd => pd.ProductDataCategories)
                    .ThenInclude(pdc => pdc.Category)
            .Select(r => Map(r))
            .ToListAsync();
        return new ReceiptListResponseDto
        {
            CurrentCount = receipts.Count,
            MaxAllowed = await _groupService.GetGroupMaxReceiptsAsync(groupId),
            Receipts = receipts
        };
    }

    public async Task<List<ReceiptDto>> GetReceiptsBySellerAsync(int groupId, string sellerId)
    {
        return await _context.Receipts
            .Where(r => r.GroupId == groupId && r.SellerId == sellerId)
            .Include(r => r.CreatedByUser)
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
            .Include(r => r.CreatedByUser)
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
            .Include(r => r.CreatedByUser)
            .Include(x => x.Seller)
            .Include(x => x.ProductEntries)
                .ThenInclude(pe => pe.ProductData).ThenInclude(pd => pd.ProductDataCategories)
                    .ThenInclude(pdc => pdc.Category)
            .FirstOrDefaultAsync();
        if (r == null)
            return null;

        return Map(r);
    }

    public async Task<CreateReceiptDto?> ParseXMLReceiptAsync(int groupId, IFormFile file)
    {
        var parser = new ReceiptXmlParser();
        var parsed = await parser.ParseAsync(file.OpenReadStream());

        if (parsed == null || parsed.Products == null || !parsed.Products.Any())
            return parsed;

        try
        {
            var productNames = parsed.Products.Select(p => p.Name).ToList();
            var request = new PredictionRequest(productNames);

            var (correlationId, replyTask) = _pendingPredictions.Register(TimeSpan.FromSeconds(5));
            await _messageBus.PublishAsync(request,
                new DeliveryOptions().WithHeader(MLMessagingConstants.CorrelationIdHeader, correlationId));
            var response = await replyTask;

            if (response?.Results != null)
            {
                var predictionDict = response.Results.ToDictionary(r => r.Text, r => r.Category);
                
                foreach (var product in parsed.Products)
                {
                    if (predictionDict.TryGetValue(product.Name, out var category) && !string.IsNullOrEmpty(category))
                    {
                        product.Categories = new List<string> { category };
                    }
                }
            }
        }
        catch (Exception)
        {
            // If ML service is down or RabbitMQ times out, fail gracefully
            // The XML parsing will still succeed, just without predicted categories
        }

        return parsed;
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
        var role = await _groupMemberService.GetUserRoleInGroupAsync(groupId, userId);
        var isAuthor = receipt.CreatedByUserId == userId;
        var isAdminOrOwner = role.HasValue && role.Value <= GroupRole.Admin;
        if (!isAuthor && !isAdminOrOwner)
            throw new ForbiddenException(ErrorMessages.UnauthorizedAccess);

        bool changed = false;
        var now = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(dto.SellerId))
            throw new BadRequestException(ErrorMessages.SellerIdRequired);

        if (!InputValidator.IsNumericString(dto.SellerId))
            throw new BadRequestException(ErrorMessages.InvalidSellerId);

        if (dto.SellerId != receipt.SellerId)
        {
            var existingSeller = await _sellerService.GetSellerAsync(groupId, dto.SellerId);
            if (existingSeller == null)
            {
                await _sellerService.CreateSellerAsync(
                    groupId, 
                    new CreateSellerDto { Id = dto.SellerId, Name = null }
                );
            }

            receipt.SellerId = dto.SellerId;
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

            var categories = await _categoryService.GetOrCreateCategoriesAsync(groupId, prod.Categories ?? new List<string>());
            if (categories.Count > ServiceConstants.MaxCategoriesPerProduct)
                throw new BadRequestException(ErrorMessages.TooManyProductCategories);

            var productData = await _productDataService.FindOrCreateProductDataAsync(groupId, prod.Name.Trim(), categories.Select(c => c.Id).ToList());

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

        var computedTotal = FinancialCalculator.CalculateReceiptTotal(receipt.ProductEntries);
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

        var role = await _groupMemberService.GetUserRoleInGroupAsync(groupId, userId);
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
            Name = pe.ProductData?.Name!,
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
            CreatedByUserName = r.CreatedByUser?.UserName,
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
            .Include(r => r.CreatedByUser)
            .Include(x => x.Seller)
            .Include(x => x.ProductEntries)
                .ThenInclude(pe => pe.ProductData).ThenInclude(pd => pd.ProductDataCategories)
                    .ThenInclude(pdc => pdc.Category)
            .FirstOrDefaultAsync(x => x.GroupId == groupId && x.Id == receiptId);

        if (r == null)
            throw new NotFoundException(ErrorMessages.ReceiptNotFound);

        return Map(r);
    }
}