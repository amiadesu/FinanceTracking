using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanceTracking.API.Services;
using FinanceTracking.API.Extensions;
using FinanceTracking.API.DTOs;
using FinanceTracking.API.Attributes;
using FinanceTracking.API.Models;
using FinanceTracking.API.Filters;

namespace FinanceTracking.API.Controllers;

[Authorize]
[ApiController]
[Route("api/groups/{groupId}/members")]
public class GroupMembersController : ControllerBase
{
    private readonly GroupMemberService _memberService;
    private readonly IGroupService _groupService;

    public GroupMembersController(
        GroupMemberService memberService, 
        IGroupService groupService)
    {
        _memberService = memberService;
        _groupService = groupService;
    }

    [HttpGet]
    [RequireGroupMembership]
    public async Task<IActionResult> GetGroupMembers(int groupId)
    {
        var members = await _memberService.GetGroupMembersAsync(groupId);
        return Ok(members);
    }

    [HttpGet("{userId}")]
    [RequireGroupMembership]
    public async Task<IActionResult> GetGroupMember(int groupId, Guid userId)
    {
        var member = await _memberService.GetGroupMemberAsync(groupId, userId);
        if (member == null)
        {
            return NotFound();
        }
        return Ok(member);
    }

    [HttpGet("me/role")]
    [RequireGroupMembership]
    public async Task<IActionResult> GetMyRole(int groupId)
    {
        var role = await _memberService.GetUserRoleInGroupAsync(groupId, User.GetUserId());
        if (role == null) {
            return NotFound();
        }
        return Ok(new { Role = role });
    }

    [HttpPatch("{userId}/role")]
    [RequireGroupMembership]
    [NotThemselves]
    [RequireGroupRole(GroupRole.Owner)]
    [ServiceFilter(typeof(ValidationFilter<UpdateGroupMemberRoleDto>))]
    public async Task<IActionResult> UpdateGroupMemberRole(int groupId, Guid userId, [FromBody] UpdateGroupMemberRoleDto dto)
    {
        if (dto.Role.HasValue && dto.Role.Value == GroupRole.Owner)
        {
            return BadRequest(new { message = Constants.ErrorMessages.UseTransferEndpoint });
        }

        var actionUserId = User.GetUserId();

        var member = await _memberService.UpdateGroupMemberRoleAsync(groupId, actionUserId, userId, dto);
        if (member == null)
        {
            return NotFound();
        }
        return Ok(member);
    }

    [HttpPatch("{userId}/transfer-ownership")]
    [RequireGroupMembership]
    [NotThemselves]
    [RequireGroupRole(GroupRole.Owner)]
    public async Task<IActionResult> TransferOwnership(int groupId, Guid userId)
    {
        var member = await _memberService.TransferOwnershipAsync(groupId, userId);
        if (member == null)
        {
            return NotFound();
        }
        return Ok(member);
    }

    [HttpDelete("{userId}")]
    [RequireGroupMembership]
    [NotThemselves]
    [RequireGroupRole(GroupRole.Admin)]
    public async Task<IActionResult> RemoveGroupMember(int groupId, Guid userId)
    {
        var actionUserId = User.GetUserId();

        var authorRole = await _memberService.GetUserRoleInGroupAsync(groupId, actionUserId);
        var userRole = await _memberService.GetUserRoleInGroupAsync(groupId, userId);
        if (authorRole == null || userRole == null || authorRole >= userRole)
        {
            return Forbid();
        }

        var success = await _memberService.RemoveGroupMemberAsync(groupId, actionUserId, userId);
        if (!success)
        {
            return NotFound();
        }
        return Ok();
    }
}