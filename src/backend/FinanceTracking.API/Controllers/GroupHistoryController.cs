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
    private readonly GroupHistoryExportService _exportService;
    private readonly GroupService _groupService;

    public GroupHistoryController(
        GroupHistoryService historyService, 
        GroupHistoryExportService exportService,
        GroupService groupService)
    {
        _historyService = historyService;
        _exportService = exportService;
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

    [HttpGet("export")]
    [RequireGroupMembership]
    [RequireGroupRole(GroupRole.Admin)]
    public async Task<IActionResult> ExportGroupHistory(
        int groupId, 
        [FromQuery] string fileType = "xlsx")
    {
        fileType = fileType.ToLower();
        if (fileType != "xlsx" && fileType != "docx")
        {
            return BadRequest("Invalid file type. Supported types are 'xlsx' and 'docx'.");
        }

        var historyData = await _historyService.GetAllGroupHistoryAsync(groupId);

        byte[] fileBytes;
        string contentType;
        string fileExtension;

        if (fileType == "xlsx")
        {
            fileBytes = _exportService.ExportToExcel(historyData);
            contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            fileExtension = "xlsx";
        }
        else
        {
            fileBytes = _exportService.ExportToWord(historyData);
            contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            fileExtension = "docx";
        }

        var fileName = $"Group_{groupId}_History_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{fileExtension}";

        return File(fileBytes, contentType, fileName);
    }
}