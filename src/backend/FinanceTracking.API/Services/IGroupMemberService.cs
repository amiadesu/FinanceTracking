using FinanceTracking.API.Models;
using FinanceTracking.API.DTOs;

namespace FinanceTracking.API.Services;

public interface IGroupMemberService
{
    Task<GroupMemberListResponseDto> GetGroupMembersAsync(int groupId);
    Task<GroupMemberDto?> GetGroupMemberAsync(int groupId, Guid userId);
    Task<bool> IsUserActiveMemberAsync(int groupId, Guid userId);
    Task<GroupRole?> GetUserRoleInGroupAsync(int groupId, Guid userId);
    Task<GroupMemberDto?> UpdateGroupMemberRoleAsync(int groupId, Guid actionUserId, Guid userId, UpdateGroupMemberRoleDto dto);
    Task<GroupMemberDto?> TransferOwnershipAsync(int groupId, Guid newOwnerUserId);
    Task<bool> RemoveGroupMemberAsync(int groupId, Guid actionUserId, Guid userId);
    Task<bool> LeaveGroupAsync(int groupId, Guid userId);
}