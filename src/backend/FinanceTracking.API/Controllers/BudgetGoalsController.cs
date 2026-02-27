using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanceTracking.API.Services;
using FinanceTracking.API.Extensions;
using FinanceTracking.API.DTOs;
using FinanceTracking.API.Exceptions;
using FinanceTracking.API.Attributes;
using FinanceTracking.API.Models;

namespace FinanceTracking.API.Controllers;

[Authorize]
[ApiController]
[Route("api/groups/{groupId}/budget-goals")]
public class BudgetGoalsController : ControllerBase
{
    private readonly BudgetGoalService _goalService;
    private readonly GroupService _groupService;

    public BudgetGoalsController(BudgetGoalService goalService, GroupService groupService)
    {
        _goalService = goalService;
        _groupService = groupService;
    }

    [HttpPost]
    [RequireGroupMembership]
    [RequireGroupRole(GroupRole.Admin)]
    public async Task<IActionResult> CreateGoal(int groupId, [FromBody] CreateBudgetGoalDto dto)
    {
        try
        {
            var result = await _goalService.CreateBudgetGoalAsync(groupId, dto);
            return CreatedAtAction(nameof(GetGoal), new { groupId = groupId, goalId = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    [RequireGroupMembership]
    public async Task<IActionResult> GetGoals(int groupId)
    {
        var goals = await _goalService.GetBudgetGoalsAsync(groupId);
        return Ok(goals);
    }

    [HttpGet("{goalId}")]
    [RequireGroupMembership]
    public async Task<IActionResult> GetGoal(int groupId, int goalId)
    {
        var goal = await _goalService.GetBudgetGoalAsync(groupId, goalId);
        if (goal == null)
            return NotFound();

        return Ok(goal);
    }

    [HttpGet("{goalId}/progress")]
    [RequireGroupMembership]
    public async Task<IActionResult> GetProgress(int groupId, int goalId)
    {
        try
        {
            var progress = await _goalService.GetBudgetGoalProgressAsync(groupId, goalId);
            return Ok(progress);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPatch("{goalId}")]
    [RequireGroupMembership]
    [RequireGroupRole(GroupRole.Admin)]
    public async Task<IActionResult> UpdateGoal(int groupId, int goalId, [FromBody] UpdateBudgetGoalDto dto)
    {
        try
        {
            var updated = await _goalService.UpdateBudgetGoalAsync(groupId, goalId, dto);
            return Ok(updated);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{goalId}")]
    [RequireGroupMembership]
    [RequireGroupRole(GroupRole.Admin)]
    public async Task<IActionResult> DeleteGoal(int groupId, int goalId)
    {
        try
        {
            await _goalService.DeleteBudgetGoalAsync(groupId, goalId);
            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }
}
