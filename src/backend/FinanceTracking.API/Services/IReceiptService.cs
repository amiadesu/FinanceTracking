using FinanceTracking.API.Models;
using FinanceTracking.API.DTOs;

namespace FinanceTracking.API.Services;

public interface IReceiptService
{
    Task<ReceiptDto> CreateReceiptAsync(int groupId, Guid? creatorId, CreateReceiptDto dto);
    Task<ReceiptListResponseDto> GetReceiptsAsync(int groupId);
    Task<List<ReceiptDto>> GetReceiptsBySellerAsync(int groupId, string sellerId);
    Task<List<ReceiptDto>> GetReceiptsByProductDataAsync(int groupId, int productDataId);
    Task<ReceiptDto?> GetReceiptAsync(int groupId, int receiptId);
    Task<CreateReceiptDto?> ParseXMLReceiptAsync(int groupId, IFormFile file);
    Task<ReceiptDto> UpdateReceiptAsync(int groupId, int receiptId, Guid userId, UpdateReceiptDto dto);
    Task DeleteReceiptAsync(int groupId, int receiptId, Guid userId);
}