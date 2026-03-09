using FinanceTracking.API.Models;
using FinanceTracking.API.DTOs;

namespace FinanceTracking.API.Services;

public interface IProductDataService
{
    Task<List<ProductDataDto>> GetProductsAsync(int groupId);
    Task<ProductDataDto?> GetProductAsync(int groupId, int productId);
    Task<ProductData> FindOrCreateProductDataAsync(int groupId, string name, List<int> categoryIds);
    Task<ProductDataDto?> UpdateProductAsync(int groupId, int productId, UpdateProductDataDto dto);
    Task<bool> DeleteProductAsync(int groupId, int productId);
}