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
[Route("api/groups/{groupId}/categories")]
public class CategoriesController : ControllerBase
{
    private readonly CategoryService _categoryService;

    public CategoriesController(CategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpPost]
    [RequireGroupMembership]
    [RequireGroupRole(GroupRole.Admin)]
    public async Task<IActionResult> CreateCategory(int groupId, [FromBody] CreateCategoryDto dto)
    {
        try
        {
            var result = await _categoryService.CreateCategoryAsync(groupId, dto);
            return CreatedAtAction(nameof(GetCategory), new { groupId = groupId, categoryId = result.Id }, result);
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    [RequireGroupMembership]
    public async Task<IActionResult> GetCategories(int groupId)
    {
        var categories = await _categoryService.GetCategoriesAsync(groupId);
        return Ok(categories);
    }

    [HttpGet("{categoryId}")]
    [RequireGroupMembership]
    public async Task<IActionResult> GetCategory(int groupId, int categoryId)
    {
        var category = await _categoryService.GetCategoryAsync(groupId, categoryId);
        
        if (category == null)
            return NotFound();

        return Ok(category);
    }

    [HttpPatch("{categoryId}")]
    [RequireGroupMembership]
    [RequireGroupRole(GroupRole.Admin)]
    public async Task<IActionResult> UpdateCategory(int groupId, int categoryId, [FromBody] UpdateCategoryDto dto)
    {
        try
        {
            var updated = await _categoryService.UpdateCategoryAsync(groupId, categoryId, dto);
            return Ok(updated);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (ForbiddenException ex)
        {
            return Forbid();
        }
        catch (BadRequestException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{categoryId}")]
    [RequireGroupMembership]
    [RequireGroupRole(GroupRole.Admin)]
    public async Task<IActionResult> DeleteCategory(int groupId, int categoryId)
    {
        try
        {
            await _categoryService.DeleteCategoryAsync(groupId, categoryId);
            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (ForbiddenException ex)
        {
            return Forbid();
        }
    }
}
