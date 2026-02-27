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

public class BudgetGoalService
{
    private readonly FinanceDbContext _context;

    public BudgetGoalService(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<BudgetGoalDto> CreateBudgetGoalAsync(int groupId, CreateBudgetGoalDto dto)
    {
        if (dto.EndDate <= dto.StartDate)
            throw new BadRequestException(ErrorMessages.InvalidBudgetPeriod);

        var now = DateTime.UtcNow;
        var goal = new BudgetGoal
        {
            GroupId = groupId,
            TargetAmount = dto.TargetAmount,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            CreatedDate = now,
            UpdatedDate = now
        };

        _context.BudgetGoals.Add(goal);
        await _context.SaveChangesAsync();

        return Map(goal);
    }

    public async Task<List<BudgetGoalDto>> GetBudgetGoalsAsync(int groupId)
    {
        return await _context.BudgetGoals
            .Where(g => g.GroupId == groupId)
            .Select(g => Map(g))
            .ToListAsync();
    }

    public async Task<BudgetGoalDto?> GetBudgetGoalAsync(int groupId, int goalId)
    {
        var goal = await _context.BudgetGoals
            .FirstOrDefaultAsync(g => g.GroupId == groupId && g.Id == goalId);

        return goal == null ? null : Map(goal);
    }

    public async Task DeleteBudgetGoalAsync(int groupId, int goalId)
    {
        var goal = await _context.BudgetGoals
            .FirstOrDefaultAsync(g => g.GroupId == groupId && g.Id == goalId);

        if (goal == null)
            throw new NotFoundException(ErrorMessages.BudgetGoalNotFound);

        _context.BudgetGoals.Remove(goal);
        await _context.SaveChangesAsync();
    }

    public async Task<BudgetGoalDto> UpdateBudgetGoalAsync(int groupId, int goalId, UpdateBudgetGoalDto dto)
    {
        var goal = await _context.BudgetGoals
            .FirstOrDefaultAsync(g => g.GroupId == groupId && g.Id == goalId);

        if (goal == null)
            throw new NotFoundException(ErrorMessages.BudgetGoalNotFound);

        bool changed = false;

        if (dto.TargetAmount.HasValue)
        {
            goal.TargetAmount = dto.TargetAmount.Value;
            changed = true;
        }

        if (dto.StartDate.HasValue)
        {
            goal.StartDate = dto.StartDate.Value;
            changed = true;
        }

        if (dto.EndDate.HasValue)
        {
            goal.EndDate = dto.EndDate.Value;
            changed = true;
        }

        if (dto.StartDate.HasValue || dto.EndDate.HasValue)
        {
            if (goal.EndDate <= goal.StartDate)
                throw new BadRequestException(ErrorMessages.InvalidBudgetPeriod);
        }

        if (changed)
        {
            goal.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return Map(goal);
    }

    public async Task<BudgetGoalProgressDto> GetBudgetGoalProgressAsync(int groupId, int goalId)
    {
        var goal = await _context.BudgetGoals
            .FirstOrDefaultAsync(g => g.GroupId == groupId && g.Id == goalId);

        if (goal == null)
            throw new NotFoundException(ErrorMessages.BudgetGoalNotFound);

        var currentAmount = await _context.Receipts
            .Where(r => r.GroupId == groupId
                        && r.PaymentDate.HasValue
                        && r.PaymentDate.Value >= goal.StartDate
                        && r.PaymentDate.Value <= goal.EndDate
                        && r.TotalAmount.HasValue)
            // TotalAmount is not null because of the filter
            .SumAsync(r => r.TotalAmount ?? 0m);

        return new BudgetGoalProgressDto
        {
            GoalId = goalId,
            GroupId = groupId,
            TargetAmount = goal.TargetAmount,
            CurrentAmount = currentAmount,
            StartDate = goal.StartDate,
            EndDate = goal.EndDate
        };
    }

    private static BudgetGoalDto Map(BudgetGoal g) => new BudgetGoalDto
    {
        Id = g.Id,
        GroupId = g.GroupId,
        TargetAmount = g.TargetAmount,
        StartDate = g.StartDate,
        EndDate = g.EndDate,
        CreatedDate = g.CreatedDate,
        UpdatedDate = g.UpdatedDate
    };
}
