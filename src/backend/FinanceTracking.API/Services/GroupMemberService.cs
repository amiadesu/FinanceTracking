using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FinanceTracking.API.Data;
using FinanceTracking.API.Models;
using FinanceTracking.API.DTOs;

namespace FinanceTracking.API.Services;

public class GroupMemberService
{
    private readonly FinanceDbContext _dbContext;
    private readonly IGroupService _groupService;
    private readonly IGroupHistoryService _historyService;

    public GroupMemberService(
        FinanceDbContext dbContext,
        IGroupService groupService,
        IGroupHistoryService historyService)
    {
        _dbContext = dbContext;
        _groupService = groupService;
        _historyService = historyService;
    }

    public async Task<GroupMemberListResponseDto> GetGroupMembersAsync(int groupId)
    {
        var members = await _dbContext.GroupMembers
            .Include(m => m.User)
            .Where(m => m.GroupId == groupId && m.Active)
            .Select(m => Map(m))
            .ToListAsync();
        return new GroupMemberListResponseDto
        {
            CurrentCount = members.Count,
            MaxAllowed = await _groupService.GetGroupMaxMembersAsync(groupId),
            GroupMembers = members
        };
    }

    public async Task<GroupMemberDto?> GetGroupMemberAsync(int groupId, Guid userId)
    {
        var member = await _dbContext.GroupMembers
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId && m.Active);
        return member == null ? null : Map(member);
    }

    public async Task<bool> IsUserActiveMemberAsync(int groupId, Guid userId)
    {
        return await _dbContext.GroupMembers
            .AnyAsync(m => m.GroupId == groupId && m.UserId == userId && m.Active);
    }

    public async Task<GroupRole?> GetUserRoleInGroupAsync(int groupId, Guid userId)
    {
        var member = await _dbContext.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId && m.Active);
        return member?.RoleId;
    }

    public async Task<GroupMemberDto?> UpdateGroupMemberRoleAsync(int groupId, Guid actionUserId, Guid userId, UpdateGroupMemberRoleDto dto)
    {
        var member = await _dbContext.GroupMembers
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId && m.Active);
        if (member == null) return null;

        if (dto.Role.HasValue && member.RoleId != dto.Role.Value) {
            _historyService.AddHistoryRecord(
                groupId: groupId,
                targetUserId: userId,
                actionUserId: actionUserId,
                note: Constants.HistoryNotes.RoleChanged,
                roleBefore: member.RoleId,
                roleAfter: dto.Role);

            member.RoleId = dto.Role.Value;
            
            member.UpdatedDate = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }

        return Map(member);
    }

    public async Task<GroupMemberDto?> TransferOwnershipAsync(int groupId, Guid newOwnerUserId)
    {
        var currentOwner = await _dbContext.GroupMembers
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.RoleId == GroupRole.Owner && m.Active);
        var newOwner = await _dbContext.GroupMembers
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == newOwnerUserId && m.Active);

        if (currentOwner == null || newOwner == null) return null;

        _historyService.AddHistoryRecord(
            groupId: groupId,
            targetUserId: newOwnerUserId,
            actionUserId: currentOwner.UserId,
            note: Constants.HistoryNotes.OwnershipTransferred,
            roleBefore: newOwner.RoleId,
            roleAfter: GroupRole.Owner);

        currentOwner.RoleId = GroupRole.Admin;
        currentOwner.UpdatedDate = DateTime.UtcNow;

        newOwner.RoleId = GroupRole.Owner;
        newOwner.UpdatedDate = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return Map(newOwner);
    }

    public async Task<bool> RemoveGroupMemberAsync(int groupId, Guid actionUserId, Guid userId)
    {
        var member = await _dbContext.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId && m.Active);
        if (member == null) return false;

        _historyService.AddHistoryRecord(
            groupId: groupId,
            targetUserId: userId,
            actionUserId: actionUserId,
            note: Constants.HistoryNotes.MemberRemoved,
            roleBefore: member.RoleId,
            roleAfter: GroupRole.Member, // Reset role to default for historical accuracy
            activeBefore: member.Active,
            activeAfter: false);

        member.Active = false;
        member.RoleId = GroupRole.Member;
        member.UpdatedDate = DateTime.UtcNow;
        
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> LeaveGroupAsync(int groupId, Guid userId)
    {
        var member = await _dbContext.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId && m.Active);
        if (member == null) return false;

        _historyService.AddHistoryRecord(
            groupId: groupId,
            targetUserId: userId,
            actionUserId: userId,
            note: Constants.HistoryNotes.MemberLeft,
            roleBefore: member.RoleId,
            roleAfter: GroupRole.Member, // Reset role to default for historical accuracy
            activeBefore: member.Active,
            activeAfter: false);

        member.Active = false;
        member.RoleId = GroupRole.Member;
        member.UpdatedDate = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    private static GroupMemberDto Map(GroupMember m) => new GroupMemberDto
    {
        UserId = m.UserId,
        UserName = m.User.UserName,
        Role = m.RoleId,
        Active = m.Active,
        JoinedDate = m.JoinedDate
    };
}