using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanceTracking.API.Services;
using FinanceTracking.API.Extensions;
using FinanceTracking.API.DTOs;
using FinanceTracking.API.Attributes;
using FinanceTracking.API.Models;

namespace FinanceTracking.API.Controllers;

[Authorize]
[ApiController]
[Route("api/groups/{groupId}/invitations")]
public class GroupInvitationsController : ControllerBase
{
    private readonly GroupInvitationService _invitationService;
    private readonly GroupService _groupService;

    public GroupInvitationsController(
        GroupInvitationService invitationService, 
        GroupService groupService)
    {
        _invitationService = invitationService;
        _groupService = groupService;
    }

    [HttpGet]
    [RequireGroupMembership]
    public async Task<IActionResult> GetGroupInvitations(int groupId)
    {
        var invitations = await _invitationService.GetGroupInvitationsAsync(groupId);
        return Ok(invitations);
    }

    [HttpPost]
    [RequireGroupMembership]
    [RequireGroupRole(GroupRole.Admin)]
    public async Task<IActionResult> CreateInvitation(int groupId, [FromBody] CreateInvitationDto dto)
    {
        var userId = User.GetUserId();

        try
        {
            var result = await _invitationService.CreateInvitationAsync(groupId, userId, dto);
            
            // Returns a 201 Created and points to the GetSpecificInvitation route
            return CreatedAtAction(
                actionName: nameof(InvitationsController.GetSpecificInvitation), 
                controllerName: "Invitations", 
                routeValues: new { id = result.Id }, 
                value: result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{invitationId}")]
    [RequireGroupMembership]
    [RequireGroupRole(GroupRole.Admin)]
    public async Task<IActionResult> CancelInvitation(int groupId, Guid invitationId)
    {
        var userId = User.GetUserId();

        try
        {
            await _invitationService.CancelInvitationAsync(groupId, invitationId, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}