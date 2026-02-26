using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanceTracking.API.Services;
using FinanceTracking.API.Extensions;

namespace FinanceTracking.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class GroupsController : ControllerBase
{
    private readonly GroupService _groupService;
    private readonly GroupHistoryService _historyService;

    public GroupsController(
        GroupService groupService,
        GroupHistoryService historyService)
    {
        _groupService = groupService;
        _historyService = historyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyGroups()
    {
        var userId = User.GetUserId();
        var groups = await _groupService.GetUserGroupsAsync(userId);
        
        return Ok(groups);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetGroup(int id)
    {
        var userId = User.GetUserId();

        if (!await _groupService.IsUserActiveMemberAsync(id, userId))
        {
            return Forbid();
        }

        var group = await _groupService.GetGroupByIdAsync(id);
        
        if (group == null)
        {
            return NotFound();
        }

        return Ok(group);
    }

    [HttpGet("{id}/members")]
    public async Task<IActionResult> GetGroupMembers(int id)
    {
        var userId = User.GetUserId();

        if (!await _groupService.IsUserActiveMemberAsync(id, userId))
        {
            return Forbid();
        }

        var members = await _groupService.GetGroupMembersAsync(id);
        return Ok(members);
    }

    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetGroupHistory(int id)
    {
        var userId = User.GetUserId();

        if (!await _groupService.IsUserActiveMemberAsync(id, userId))
        {
            return Forbid();
        }

        var history = await _historyService.GetGroupHistoryAsync(id);
        return Ok(history);
    }
}