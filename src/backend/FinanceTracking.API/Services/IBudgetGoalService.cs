using FinanceTracking.API.Models;
using FinanceTracking.API.DTOs;

namespace FinanceTracking.API.Services;

public interface IBudgetGoalService
{
    Task<BudgetGoalDto> CreateBudgetGoalAsync(int groupId, CreateBudgetGoalDto dto);
    Task<BudgetGoalListResponseDto> GetBudgetGoalsAsync(int groupId);
    Task<BudgetGoalDto?> GetBudgetGoalAsync(int groupId, int goalId);
    Task DeleteBudgetGoalAsync(int groupId, int goalId);
    Task<BudgetGoalDto> UpdateBudgetGoalAsync(int groupId, int goalId, UpdateBudgetGoalDto dto);
    Task<BudgetGoalProgressDto> GetBudgetGoalProgressAsync(int groupId, int goalId);
}