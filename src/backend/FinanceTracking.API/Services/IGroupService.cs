using FinanceTracking.API.Models;
using FinanceTracking.API.DTOs;

namespace FinanceTracking.API.Services;

public interface IGroupService
{
    Task<Group> CreateGroupAsync(AppUser owner, string name, bool isPersonal);
    Task<GroupDto> CreateGroupAsync(Guid creatorId, string name, bool isPersonal);
    Task<GroupListResponseDto> GetUserGroupsAsync(Guid userId);
    Task<GroupDto?> GetGroupByIdAsync(int groupId);
    Task<bool> IsGroupFullAsync(int groupId);
    Task<bool> IsGroupPersonalAsync(int groupId);
    Task<int> GetGroupMaxMembersAsync(int groupId);
    Task<int> GetMaxGroupsPerUserAsync(Guid userId);
    Task<int> GetGroupMaxBudgetGoalsAsync(int groupId);
    Task<int> GetGroupMaxCategoriesAsync(int groupId);
    Task<int> GetGroupMaxReceiptsAsync(int groupId);
    Task<int> GetGroupMaxSellersAsync(int groupId);
    Task<GroupDto> UpdateGroupAsync(int groupId, Guid userId, string newName);
    Task<GroupDto> ResetGroupAsync(int groupId, Guid userId, ResetGroupDto resetData);
    Task DeleteGroupAsync(int groupId);
}