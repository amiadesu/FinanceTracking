using System;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracking.API.DTOs;

public class StatisticsFilterDto
{
    [FromQuery(Name = "startDate")]
    public DateTime StartDate { get; set; }
    [FromQuery(Name = "endDate")]
    public DateTime EndDate { get; set; }
    [FromQuery(Name = "personalOnly")]
    public bool IsPersonalBudgetOnly { get; set; } 
    [FromQuery(Name = "sellerId")]
    public string? SellerId { get; set; }
    [FromQuery(Name = "categoryId")]
    public int? CategoryId { get; set; }
    [FromQuery(Name = "top")]
    public int Top { get; set; } = 10;
}

public class ProductStatisticDto
{
    public string ProductName { get; set; } = string.Empty;
    public decimal TotalQuantity { get; set; }
    public decimal TotalSpent { get; set; }
}

public class SpendingHistoryDataPointDto
{
    public DateTime Date { get; set; }
    public decimal TotalSpent { get; set; }
}