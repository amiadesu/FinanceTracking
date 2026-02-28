using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanceTracking.API.Services;
using FinanceTracking.API.Extensions;
using FinanceTracking.API.DTOs;
using FinanceTracking.API.Exceptions;
using FinanceTracking.API.Attributes;

namespace FinanceTracking.API.Controllers;

[Authorize]
[ApiController]
[Route("api/groups/{groupId}/receipts")]
public class ReceiptsController : ControllerBase
{
    private readonly ReceiptService _receiptService;

    public ReceiptsController(ReceiptService receiptService)
    {
        _receiptService = receiptService;
    }

    [HttpPost]
    [RequireGroupMembership]
    public async Task<IActionResult> CreateReceipt(int groupId, [FromBody] CreateReceiptDto dto)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await _receiptService.CreateReceiptAsync(groupId, userId, dto);
            return CreatedAtAction(nameof(GetReceipt), new { groupId = groupId, receiptId = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    [RequireGroupMembership]
    public async Task<IActionResult> GetReceipts(int groupId)
    {
        var receipts = await _receiptService.GetReceiptsAsync(groupId);
        return Ok(receipts);
    }

    [HttpGet("{receiptId}")]
    [RequireGroupMembership]
    public async Task<IActionResult> GetReceipt(int groupId, int receiptId)
    {
        var receipt = await _receiptService.GetReceiptAsync(groupId, receiptId);
        if (receipt == null)
            return NotFound();

        return Ok(receipt);
    }

    [HttpPatch("{receiptId}")]
    [RequireGroupMembership]
    public async Task<IActionResult> UpdateReceipt(int groupId, int receiptId, [FromBody] UpdateReceiptDto dto)
    {
        try
        {
            var userId = User.GetUserId();
            var updated = await _receiptService.UpdateReceiptAsync(groupId, receiptId, userId, dto);
            return Ok(updated);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (ForbiddenException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{receiptId}")]
    [RequireGroupMembership]
    public async Task<IActionResult> DeleteReceipt(int groupId, int receiptId)
    {
        try
        {
            var userId = User.GetUserId();
            await _receiptService.DeleteReceiptAsync(groupId, receiptId, userId);
            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (ForbiddenException)
        {
            return Forbid();
        }
    }
}