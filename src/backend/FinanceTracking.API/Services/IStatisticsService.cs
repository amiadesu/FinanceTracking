using FinanceTracking.API.Models;
using FinanceTracking.API.DTOs;

namespace FinanceTracking.API.Services;

public interface IStatisticsService
{
    Task<List<ProductStatisticDto>> GetTopProductsAsync(int groupId, Guid userId, StatisticsFilterDto filter);
    Task<List<SpendingHistoryDataPointDto>> GetSpendingHistoryAsync(int groupId, Guid userId, StatisticsFilterDto filter);
}