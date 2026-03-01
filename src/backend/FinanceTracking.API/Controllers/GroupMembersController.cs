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
[Route("api/groups/{groupId}/members")]
public class GroupMembersController : ControllerBase
{
    private readonly GroupMemberService _memberService;
    private readonly GroupService _groupService;

    public GroupMembersController(
        GroupMemberService memberService, 
        GroupService groupService)
    {
        _memberService = memberService;
        _groupService = groupService;
    }

    [HttpGet("{groupId}/members")]
    [RequireGroupMembership]
    public async Task<IActionResult> GetGroupMembers(int groupId)
    {
        var members = await _groupService.GetGroupMembersAsync(groupId);
        return Ok(members);
    }
}