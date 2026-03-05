using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanceTracking.API.Services;
using FinanceTracking.API.Extensions;
using FinanceTracking.API.Attributes;
using FinanceTracking.API.Models;
using FinanceTracking.API.DTOs;
using FinanceTracking.API.Filters;

namespace FinanceTracking.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class GroupsController : ControllerBase
{
    private readonly GroupService _groupService;
    private readonly GroupHistoryService _historyService;
    private readonly GroupMemberService _memberService;

    public GroupsController(
        GroupService groupService,
        GroupHistoryService historyService,
        GroupMemberService memberService)
    {
        _groupService = groupService;
        _historyService = historyService;
        _memberService = memberService;
    }

    [HttpPost]
    [ServiceFilter(typeof(ValidationFilter<CreateGroupDto>))]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupDto dto)
    {
        var userId = User.GetUserId();

        try
        {
            var group = await _groupService.CreateGroupAsync(userId, dto.Name, isPersonal: false);
            return Ok(group);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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

    [HttpPatch("{groupId}")]
    [RequireGroupMembership]
    [RequireGroupRole(GroupRole.Owner)]
    [ServiceFilter(typeof(ValidationFilter<UpdateGroupDto>))]
    public async Task<IActionResult> UpdateGroup(int groupId, [FromBody] UpdateGroupDto dto)
    {
        var userId = User.GetUserId();
        var group = await _groupService.UpdateGroupAsync(groupId, userId, dto.Name);
        if (group == null)
        {
            return NotFound();
        }
        return Ok(group);
    }

    [HttpPatch("{groupId}/reset")]
    [RequireGroupMembership]
    [RequireGroupRole(GroupRole.Owner)]
    [ServiceFilter(typeof(ValidationFilter<ResetGroupDto>))]
    public async Task<IActionResult> ResetGroup(int groupId, [FromBody] ResetGroupDto dto)
    {
        var userId = User.GetUserId();
        try
        {
            await _groupService.ResetGroupAsync(groupId, userId, dto);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{groupId}")]
    [NotPersonalGroup]
    [RequireGroupMembership]
    [RequireGroupRole(GroupRole.Owner)]
    public async Task<IActionResult> DeleteGroup(int groupId)
    {
        try
        {
            await _groupService.DeleteGroupAsync(groupId);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{groupId}/leave")]
    [NotPersonalGroup]
    [RequireGroupMembership]
    [BlacklistGroupRole(GroupRole.Owner)]
    public async Task<IActionResult> LeaveGroup(int groupId)
    {
        var userId = User.GetUserId();
        try
        {
            await _memberService.LeaveGroupAsync(groupId, userId);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}