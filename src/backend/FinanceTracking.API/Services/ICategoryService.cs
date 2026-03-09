using FinanceTracking.API.Models;
using FinanceTracking.API.DTOs;

namespace FinanceTracking.API.Services;

public interface ICategoryService
{
    Task<CategoryDto> CreateCategoryAsync(int groupId, CreateCategoryDto dto);
    Task<CategoryListResponseDto> GetCategoriesAsync(int groupId);
    Task<CategoryDto?> GetCategoryAsync(int groupId, int categoryId);
    Task<List<CategoryDto>> GetSystemCategoriesAsync();
    Task<List<Category>> GetOrCreateCategoriesAsync(int groupId, List<string> names);
    Task<CategoryDto> UpdateCategoryAsync(int groupId, int categoryId, UpdateCategoryDto dto);
    Task DeleteCategoryAsync(int groupId, int categoryId);
}