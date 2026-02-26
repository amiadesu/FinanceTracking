using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanceTracking.API.Services;
using FinanceTracking.API.Extensions;

namespace FinanceTracking.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")] 
public class InvitationsController : ControllerBase
{
    private readonly GroupInvitationService _invitationService;

    public InvitationsController(GroupInvitationService invitationService)
    {
        _invitationService = invitationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyInvitations()
    {
        var userId = User.GetUserId();
        var invitations = await _invitationService.GetPendingInvitationsAsync(userId);
        
        return Ok(invitations);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSpecificInvitation(Guid id)
    {
        var userId = User.GetUserId();
        
        try
        {
            var invitation = await _invitationService.GetInvitationAsync(id, userId);
            return Ok(invitation);
        }
        catch (Exception)
        {
            return NotFound();
        }
    }

    [HttpPost("{id}/accept")]
    public async Task<IActionResult> AcceptInvitation(Guid id)
    {
        var userId = User.GetUserId();
        
        try
        {
            await _invitationService.AcceptInvitationAsync(id, userId);
            return Ok(new { message = Constants.StatusMessages.InvitationAcceptedSuccess });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectInvitation(Guid id)
    {
        var userId = User.GetUserId();
        
        try
        {
            await _invitationService.RejectInvitationAsync(id, userId);
            return Ok(new { message = Constants.StatusMessages.InvitationRejectedSuccess });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}