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

public class SellerService: ISellerService
{
    private readonly FinanceDbContext _context;
    private readonly IGroupService _groupService;

    public SellerService(
        FinanceDbContext context,
        IGroupService groupService)
    {
        _context = context;
        _groupService = groupService;
    }

    public async Task<SellerDto> CreateSellerAsync(int groupId, CreateSellerDto dto)
    {
        if (!InputValidator.IsNumericString(dto.Id))
            throw new InvalidOperationException(ErrorMessages.InvalidSellerId);
        
        var now = DateTime.UtcNow;
        var seller = new Seller
        {
            Id = dto.Id,
            GroupId = groupId,
            Name = string.IsNullOrWhiteSpace(dto.Name) ? null : dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            CreatedDate = now,
            UpdatedDate = now
        };

        _context.Sellers.Add(seller);
        await _context.SaveChangesAsync();

        return Map(seller);
    }

    public async Task<SellerListResponseDto> GetSellersAsync(int groupId)
    {
        var sellers = await _context.Sellers
            .Where(s => s.GroupId == groupId)
            .OrderBy(s => s.Name)
            .Select(s => Map(s))
            .ToListAsync();
        return new SellerListResponseDto
        {
            CurrentCount = sellers.Count,
            MaxAllowed = await _groupService.GetGroupMaxSellersAsync(groupId),
            Sellers = sellers
        };
    }

    public async Task<SellerDto?> GetSellerAsync(int groupId, string sellerId)
    {
        var seller = await _context.Sellers
            .FirstOrDefaultAsync(s => s.GroupId == groupId && s.Id == sellerId);
        return seller == null ? null : Map(seller);
    }

    public async Task<SellerDto?> UpdateSellerAsync(int groupId, string sellerId, UpdateSellerDto dto)
    {
        var seller = await _context.Sellers
            .FirstOrDefaultAsync(s => s.GroupId == groupId && s.Id == sellerId);

        if (seller == null) return null;

        if (dto.Name != null) 
            seller.Name = string.IsNullOrWhiteSpace(dto.Name) ? null : dto.Name.Trim();
        
        if (dto.Description != null) 
            seller.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();

        seller.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Map(seller);
    }

    public async Task<bool> DeleteSellerAsync(int groupId, string sellerId)
    {
        var seller = await _context.Sellers
            .FirstOrDefaultAsync(s => s.GroupId == groupId && s.Id == sellerId);

        if (seller == null) return false;

        // Check if the seller is orphaned (not used by any receipts)
        var isUsed = await _context.Receipts
            .AnyAsync(r => r.GroupId == groupId && r.SellerId == sellerId);

        if (isUsed)
            throw new InvalidOperationException(ErrorMessages.SellerNotOrphaned);

        _context.Sellers.Remove(seller);
        await _context.SaveChangesAsync();
        
        return true;
    }

    private static SellerDto Map(Seller s) => new SellerDto
    {
        Id = s.Id,
        GroupId = s.GroupId,
        Name = s.Name,
        Description = s.Description,
        CreatedDate = s.CreatedDate,
        UpdatedDate = s.UpdatedDate
    };
}
