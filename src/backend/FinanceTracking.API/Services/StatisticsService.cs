using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FinanceTracking.API.Data;
using FinanceTracking.API.DTOs;
using FinanceTracking.API.Models;
using FinanceTracking.API.Utils;

namespace FinanceTracking.API.Services;

public class StatisticsService: IStatisticsService
{
    private readonly FinanceDbContext _context;

    public StatisticsService(FinanceDbContext context)
    {
        _context = context;
    }

    private IQueryable<ProductEntry> BuildFilteredQuery(int groupId, Guid userId, StatisticsFilterDto filter)
    {
        var query = _context.ProductEntries
            .Include(pe => pe.Receipt)
            .Include(pe => pe.ProductData)
                .ThenInclude(pd => pd.ProductDataCategories)
            .Where(pe => pe.GroupId == groupId);

        var utcStart = DateTime.SpecifyKind(filter.StartDate, DateTimeKind.Utc);
        var utcEnd = DateTime.SpecifyKind(filter.EndDate, DateTimeKind.Utc);
        
        query = query.Where(pe => pe.Receipt.PaymentDate >= utcStart && pe.Receipt.PaymentDate <= utcEnd);

        if (filter.IsPersonalBudgetOnly)
        {
            query = query.Where(pe => pe.Receipt.CreatedByUserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(filter.SellerId))
        {
            query = query.Where(pe => pe.Receipt.SellerId == filter.SellerId);
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(pe => pe.ProductData.ProductDataCategories
                .Any(pdc => pdc.CategoryId == filter.CategoryId.Value));
        }

        return query;
    }

    public async Task<List<ProductStatisticDto>> GetTopProductsAsync(int groupId, Guid userId, StatisticsFilterDto filter)
    {
        var query = BuildFilteredQuery(groupId, userId, filter);

        var rawData = await query
            .Select(pe => new 
            { 
                ProductName = pe.ProductData.Name, 
                pe.Price, 
                pe.Quantity 
            })
            .ToListAsync();

        return rawData
            .GroupBy(pe => pe.ProductName)
            .Select(g => new ProductStatisticDto
            {
                ProductName = g.Key,
                TotalQuantity = g.Sum(pe => pe.Quantity),
                TotalSpent = g.Sum(pe => FinancialCalculator.CalculateEntryTotal(pe.Price, pe.Quantity))
            })
            .OrderByDescending(x => x.TotalSpent)
            .Take(filter.Top)
            .ToList();
    }

    public async Task<List<SpendingHistoryDataPointDto>> GetSpendingHistoryAsync(int groupId, Guid userId, StatisticsFilterDto filter)
    {
        var query = BuildFilteredQuery(groupId, userId, filter);

        var rawData = await query
            .Select(pe => new 
            { 
                PaymentDate = pe.Receipt.PaymentDate, 
                pe.Price, 
                pe.Quantity 
            })
            .ToListAsync();

        return rawData
            .GroupBy(pe => pe.PaymentDate.Date)
            .Select(g => new SpendingHistoryDataPointDto
            {
                Date = g.Key,
                TotalSpent = g.Sum(pe => FinancialCalculator.CalculateEntryTotal(pe.Price, pe.Quantity))
            })
            .OrderBy(x => x.Date)
            .ToList();
    }
}