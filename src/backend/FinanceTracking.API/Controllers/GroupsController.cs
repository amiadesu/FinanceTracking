using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanceTracking.API.Services;
using FinanceTracking.API.Extensions;
using FinanceTracking.API.Attributes;
using FinanceTracking.API.Models;

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

    [HttpGet("{groupId}")]
    [RequireGroupMembership]
    public async Task<IActionResult> GetGroup(int groupId)
    {
        var group = await _groupService.GetGroupByIdAsync(groupId);
        
        if (group == null)
        {
            return NotFound();
        }

        return Ok(group);
    }

    [HttpGet("{groupId}/members")]
    [RequireGroupMembership]
    public async Task<IActionResult> GetGroupMembers(int groupId)
    {
        var members = await _groupService.GetGroupMembersAsync(groupId);
        return Ok(members);
    }

    [HttpGet("{groupId}/history")]
    [RequireGroupMembership]
    [RequireGroupRole(GroupRole.Admin)]
    public async Task<IActionResult> GetGroupHistory(int groupId)
    {
        var history = await _historyService.GetGroupHistoryAsync(groupId);
        return Ok(history);
    }
}