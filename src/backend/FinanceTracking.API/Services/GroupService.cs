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
            Note = Constants.HistoryNotes.GroupCreated,
            ChangedAt = now
        };

        _dbContext.Groups.Add(group);
        _dbContext.GroupMembers.Add(groupMember);
        _dbContext.GroupMemberHistories.Add(historyEntry);

        await _dbContext.SaveChangesAsync();

        return group;
    }

    public async Task<List<GroupDto>> GetUserGroupsAsync(Guid userId)
    {
        return await _dbContext.Groups
            .Include(g => g.Members)
            .Where(g => g.Members.Any(m => m.UserId == userId && m.Active))
            .Select(g => Map(g))
            .ToListAsync();
    }

    public async Task<GroupDto?> GetGroupByIdAsync(int groupId)
    {
        var group = await _dbContext.Groups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId);
        return group == null ? null : Map(group);
    }

    public async Task<bool> IsGroupFullAsync(int groupId)
    {
        var memberCount = await _dbContext.GroupMembers
            .CountAsync(m => m.GroupId == groupId && m.Active);
        return memberCount >= await GetGroupMaxMembersAsync(groupId);
    }

    public async Task<bool> IsGroupPersonalAsync(int groupId)
    {
        var group = await _dbContext.Groups
            .FirstOrDefaultAsync(g => g.Id == groupId);
        return group?.IsPersonal ?? false;
    }

    public async Task<int> GetGroupMaxMembersAsync(int groupId)
    {
        var group = await _dbContext.Groups
            .FirstOrDefaultAsync(g => g.Id == groupId);
        return CalculateMaxMembers(group);
    }

    public async Task<int> GetGroupMaxBudgetGoalsAsync(int groupId)
    {
        return Constants.ServiceConstants.MaxBudgetGoalsPerGroup;
    }

    public async Task<int> GetGroupMaxCategoriesAsync(int groupId)
    {
        return Constants.ServiceConstants.MaxCategoriesPerGroup;
    }

    public async Task<int> GetGroupMaxReceiptsAsync(int groupId)
    {
        return Constants.ServiceConstants.MaxReceiptsPerGroup;
    }

    public async Task<int> GetGroupMaxSellersAsync(int groupId)
    {
        return Constants.ServiceConstants.MaxSellersPerGroup;
    }

    public int CalculateMaxMembers(Group? group)
    {
        if (group == null) return 0;
        
        return group.IsPersonal ? 1 : Constants.ServiceConstants.MaxMembersPerGroup;
    }

    private GroupDto Map(Group g) => new GroupDto
    {
        Id = g.Id,
        Name = g.Name,
        IsFull = g.Members.Count(m => m.Active) >= CalculateMaxMembers(g),
        IsPersonal = g.IsPersonal,
        OwnerId = g.OwnerId,
        CreatedDate = g.CreatedDate
    };
}