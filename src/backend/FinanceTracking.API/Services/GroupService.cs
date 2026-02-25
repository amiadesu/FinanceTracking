using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using FinanceTracking.API.Data;
using FinanceTracking.API.Models;
using FinanceTracking.API.DTOs;

namespace FinanceTracking.API.Services;

public class GroupService
{
    private readonly FinanceDbContext _dbContext;

    public GroupService(FinanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Group> CreateGroupAsync(AppUser creator, string name, bool isPersonal)
    {
        var now = DateTime.UtcNow;

        var group = new Group
        {
            Owner = creator,
            Name = name,
            IsPersonal = isPersonal,
            CreatedDate = now,
            UpdatedDate = now
        };

        var groupMember = new GroupMember
        {
            User = creator,
            Group = group,
            RoleId = GroupRole.Owner,
            Active = true,
            JoinedDate = now,
            UpdatedDate = now
        };

        var historyEntry = new GroupMemberHistory
        {
            User = creator,
            ChangedByUser = creator,
            Group = group,
            RoleIdAfter = GroupRole.Owner,
            ActiveAfter = true,
            Note = "created group",
            ChangedAt = now
        };

        _dbContext.Groups.Add(group);
        _dbContext.GroupMembers.Add(groupMember);
        _dbContext.GroupMemberHistories.Add(historyEntry);

        await _dbContext.SaveChangesAsync();

        return group;
    }

    public async Task<bool> IsUserActiveMemberAsync(int groupId, Guid userId)
    {
        return await _dbContext.GroupMembers
            .AnyAsync(m => m.GroupId == groupId && m.UserId == userId && m.Active);
    }

    public async Task<List<GroupDto>> GetUserGroupsAsync(Guid userId)
    {
        return await _dbContext.Groups
            .Where(g => g.Members.Any(m => m.UserId == userId && m.Active))
            .Select(g => new GroupDto(g.Id, g.Name, g.IsPersonal, g.OwnerId, g.CreatedDate))
            .ToListAsync();
    }

    public async Task<GroupDto?> GetGroupByIdAsync(int groupId)
    {
        return await _dbContext.Groups
            .Where(g => g.Id == groupId)
            .Select(g => new GroupDto(g.Id, g.Name, g.IsPersonal, g.OwnerId, g.CreatedDate))
            .FirstOrDefaultAsync();
    }

    public async Task<List<GroupMemberDto>> GetGroupMembersAsync(int groupId)
    {
        return await _dbContext.GroupMembers
            .Where(m => m.GroupId == groupId)
            .Select(m => new GroupMemberDto(
                m.UserId, 
                m.User.UserName, 
                m.RoleId, 
                m.Active, 
                m.JoinedDate))
            .ToListAsync();
    }

    public async Task<List<GroupHistoryDto>> GetGroupHistoryAsync(int groupId)
    {
        return await _dbContext.GroupMemberHistories
            .Where(h => h.GroupId == groupId)
            .OrderByDescending(h => h.ChangedAt)
            .Select(h => new GroupHistoryDto(
                h.Id, 
                h.Note, 
                h.RoleIdBefore, 
                h.RoleIdAfter, 
                h.ActiveBefore, 
                h.ActiveAfter, 
                h.ChangedAt, 
                h.User != null ? h.User.UserName : "Unknown", 
                h.ChangedByUser != null ? h.ChangedByUser.UserName : "System"))
            .ToListAsync();
    }
}