using FinanceTracking.API.Models;
using FinanceTracking.API.DTOs;

namespace FinanceTracking.API.Services;

public interface ISellerService
{
    Task<SellerDto> CreateSellerAsync(int groupId, CreateSellerDto dto);
    Task<SellerListResponseDto> GetSellersAsync(int groupId);
    Task<SellerDto?> GetSellerAsync(int groupId, string sellerId);
    Task<SellerDto?> UpdateSellerAsync(int groupId, string sellerId, UpdateSellerDto dto);
    Task<bool> DeleteSellerAsync(int groupId, string sellerId);
}