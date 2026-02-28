using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanceTracking.API.Services;
using FinanceTracking.API.DTOs;
using FinanceTracking.API.Attributes;
using FinanceTracking.API.Models;

namespace FinanceTracking.API.Controllers;

[Authorize]
[ApiController]
[Route("api/groups/{groupId}/products")]
public class ProductsController : ControllerBase
{
    private readonly ProductDataService _productService;

    public ProductsController(ProductDataService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    [RequireGroupMembership]
    public async Task<IActionResult> GetProducts(int groupId)
    {
        var products = await _productService.GetProductsAsync(groupId);
        return Ok(products);
    }

    [HttpGet("{productId}")]
    [RequireGroupMembership]
    public async Task<IActionResult> GetProduct(int groupId, int productId)
    {
        var product = await _productService.GetProductAsync(groupId, productId);
        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [HttpPatch("{productId}")]
    [RequireGroupMembership]
    [RequireGroupRole(GroupRole.Admin)]
    public async Task<IActionResult> UpdateProduct(int groupId, int productId, [FromBody] UpdateProductDataDto dto)
    {
        try
        {
            var product = await _productService.UpdateProductAsync(groupId, productId, dto);
            if (product == null)
                return NotFound();

            return Ok(product);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{productId}")]
    [RequireGroupMembership]
    [RequireGroupRole(GroupRole.Admin)]
    public async Task<IActionResult> DeleteProduct(int groupId, int productId)
    {
        try
        {
            var deleted = await _productService.DeleteProductAsync(groupId, productId);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}