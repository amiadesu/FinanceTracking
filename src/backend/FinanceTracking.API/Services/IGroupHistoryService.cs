using FinanceTracking.API.Models;
using FinanceTracking.API.DTOs;

namespace FinanceTracking.API.Services;

public interface IGroupHistoryService
{
    void AddHistoryRecord(
        int groupId, 
        Guid? targetUserId, 
        Guid actionUserId, 
        string note, 
        GroupRole? roleBefore = null, 
        GroupRole? roleAfter = null, 
        bool? activeBefore = null, 
        bool? activeAfter = null,
        string? nameBefore = null,
        string? nameAfter = null
    );
    Task<GroupHistoryListResponseDto> GetGroupHistoryAsync(int groupId, int pageNumber, int pageSize);
    Task<List<GroupHistoryDto>> GetAllGroupHistoryAsync(int groupId);
}