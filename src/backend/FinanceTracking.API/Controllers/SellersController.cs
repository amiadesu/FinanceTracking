using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanceTracking.API.Services;
using FinanceTracking.API.DTOs;
using FinanceTracking.API.Exceptions;
using FinanceTracking.API.Attributes;
using FinanceTracking.API.Models;

namespace FinanceTracking.API.Controllers;

[Authorize]
[ApiController]
[Route("api/groups/{groupId}/sellers")]
public class SellersController : ControllerBase
{
    private readonly SellerService _sellerService;

    public SellersController(SellerService sellerService)
    {
        _sellerService = sellerService;
    }

    [HttpGet]
    [RequireGroupMembership]
    public async Task<IActionResult> GetSellers(int groupId)
    {
        var sellers = await _sellerService.GetSellersAsync(groupId);
        return Ok(sellers);
    }

    [HttpGet("{sellerId}")]
    [RequireGroupMembership]
    public async Task<IActionResult> GetSeller(int groupId, int sellerId)
    {
        var seller = await _sellerService.GetSellerAsync(groupId, sellerId);
        if (seller == null)
            return NotFound();
        return Ok(seller);
    }
}
