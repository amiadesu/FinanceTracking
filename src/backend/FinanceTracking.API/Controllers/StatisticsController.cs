using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanceTracking.API.Services;
using FinanceTracking.API.DTOs;
using FinanceTracking.API.Attributes;
using FinanceTracking.API.Extensions;
using FinanceTracking.API.Filters;

namespace FinanceTracking.API.Controllers;

[Authorize]
[ApiController]
[Route("api/groups/{groupId}/statistics")]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _statisticsService;

    public StatisticsController(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    [HttpGet("top-products")]
    [RequireGroupMembership]
    [ServiceFilter(typeof(ValidationFilter<StatisticsFilterDto>))]
    public async Task<IActionResult> GetTopProducts(int groupId, [FromQuery] StatisticsFilterDto filter)
    {
        try
        {
            var userId = User.GetUserId();
            var stats = await _statisticsService.GetTopProductsAsync(groupId, userId, filter);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("spending-history")]
    [RequireGroupMembership]
    [ServiceFilter(typeof(ValidationFilter<StatisticsFilterDto>))]
    public async Task<IActionResult> GetSpendingHistory(int groupId, [FromQuery] StatisticsFilterDto filter)
    {
        try
        {
            var userId = User.GetUserId();
            var history = await _statisticsService.GetSpendingHistoryAsync(groupId, userId, filter);
            return Ok(history);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}