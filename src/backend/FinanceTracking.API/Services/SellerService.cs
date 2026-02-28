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

public class SellerService
{
    private readonly FinanceDbContext _context;

    public SellerService(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<SellerDto> CreateSellerAsync(int groupId, CreateSellerDto dto)
    {
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

    public async Task<SellerDto?> GetSellerAsync(int groupId, int sellerId)
    {
        var seller = await _context.Sellers
            .FirstOrDefaultAsync(s => s.GroupId == groupId && s.Id == sellerId);
        return seller == null ? null : Map(seller);
    }

    public async Task<List<SellerDto>> GetSellersAsync(int groupId)
    {
        return await _context.Sellers
            .Where(s => s.GroupId == groupId)
            .OrderBy(s => s.Name)
            .Select(s => Map(s))
            .ToListAsync();
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
