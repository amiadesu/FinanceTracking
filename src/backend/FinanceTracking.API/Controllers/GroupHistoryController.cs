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
[Route("api/groups/{groupId}/history")]
public class GroupHistoryController : ControllerBase
{
    private readonly GroupHistoryService _historyService;
    private readonly GroupService _groupService;

    public GroupHistoryController(
        GroupHistoryService historyService, 
        GroupService groupService)
    {
        _historyService = historyService;
        _groupService = groupService;
    }

    [HttpGet]
    [RequireGroupMembership]
    [RequireGroupRole(GroupRole.Admin)]
    public async Task<IActionResult> GetGroupHistory(
        int groupId, 
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 20)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1)
        {
            pageSize = Constants.ServiceConstants.DefaultGroupHistoryEntriesPerPage;
        }
        if (pageSize > Constants.ServiceConstants.MaxGroupHistoryEntriesPerPage)
        {
            pageSize = Constants.ServiceConstants.MaxGroupHistoryEntriesPerPage;
        }
        

        var response = await _historyService.GetGroupHistoryAsync(groupId, pageNumber, pageSize);

        return Ok(response);
    }
}