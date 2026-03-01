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
    private readonly GroupHistoryService _historyService;

    public GroupService(
        FinanceDbContext dbContext,
        GroupHistoryService historyService)
    {
        _dbContext = dbContext;
        _historyService = historyService;
    }

    public async Task<Group> CreateGroupAsync(AppUser creator, string name, bool isPersonal)
    {
        var existingPersonalGroup = await _dbContext.Groups
            .FirstOrDefaultAsync(g => g.OwnerId == creator.Id && g.IsPersonal);

        if (isPersonal && existingPersonalGroup != null)
            throw new InvalidOperationException(Constants.ErrorMessages.UserAlreadyHasPersonalGroup);

        var groupCount = await _dbContext.Groups.CountAsync(g => g.OwnerId == creator.Id);
        if (groupCount >= Constants.ServiceConstants.MaxGroupsPerUser)
            throw new InvalidOperationException(Constants.ErrorMessages.UserReachedMaxGroups);

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

    public async Task<Group> CreateGroupAsync(Guid creatorId, string name, bool isPersonal)
    {
        var existingPersonalGroup = await _dbContext.Groups
            .FirstOrDefaultAsync(g => g.OwnerId == creatorId && g.IsPersonal);

        if (isPersonal && existingPersonalGroup != null)
            throw new InvalidOperationException(Constants.ErrorMessages.UserAlreadyHasPersonalGroup);

        var groupCount = await _dbContext.Groups.CountAsync(g => g.OwnerId == creatorId);
        if (groupCount >= Constants.ServiceConstants.MaxGroupsPerUser)
            throw new InvalidOperationException(Constants.ErrorMessages.UserReachedMaxGroups);

        var creator = await _dbContext.Users.FindAsync(creatorId);
        if (creator == null)
            throw new InvalidOperationException(Constants.ErrorMessages.UserNotFound);

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

    public async Task<GroupListResponseDto> GetUserGroupsAsync(Guid userId)
    {
        var groups = await _dbContext.Groups
            .Include(g => g.Members)
            .Where(g => g.Members.Any(m => m.UserId == userId && m.Active))
            .Select(g => Map(g))
            .ToListAsync();
        return new GroupListResponseDto
        {
            CurrentCount = groups.Count,
            MaxAllowed = await GetMaxGroupsPerUserAsync(userId),
            Groups = groups
        };
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

    public async Task<int> GetMaxGroupsPerUserAsync(Guid userId)
    {
        return Constants.ServiceConstants.MaxGroupsPerUser;
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

    public async Task<GroupDto> UpdateGroupAsync(int groupId, Guid userId, string newName)
    {
        var group = await _dbContext.Groups
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
            throw new InvalidOperationException(Constants.ErrorMessages.GroupNotFound);

        if (group.Name != newName)
        {
            _historyService.AddHistoryRecord(
                groupId: groupId,
                targetUserId: null,
                actionUserId: userId,
                note: Constants.HistoryNotes.GroupRenamed,
                nameBefore: group.Name,
                nameAfter: newName
            );
            
            group.Name = newName;
            group.UpdatedDate = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        return Map(group);
    }

    public async Task<GroupDto> ResetGroupAsync(int groupId, Guid userId, ResetGroupDto resetData)
    {
        var group = await _dbContext.Groups
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
            throw new InvalidOperationException(Constants.ErrorMessages.GroupNotFound);

        if (resetData.ResetMembers)
        {
            var members = await _dbContext.GroupMembers
                .Where(m => m.GroupId == groupId && m.Active && m.UserId != userId)
                .ToListAsync();

            foreach (var member in members)
            {
                member.Active = false;
                member.UpdatedDate = DateTime.UtcNow;
            }

            _historyService.AddHistoryRecord(
                groupId: groupId,
                targetUserId: null,
                actionUserId: userId,
                note: Constants.HistoryNotes.GroupMembersReset
            );
        }

        if (resetData.ResetCategories)
        {
            var categories = await _dbContext.Categories
                .Where(c => c.GroupId == groupId)
                .ToListAsync();

            _dbContext.Categories.RemoveRange(categories);

            _historyService.AddHistoryRecord(
                groupId: groupId,
                targetUserId: null,
                actionUserId: userId,
                note: Constants.HistoryNotes.GroupCategoriesReset
            );
        }

        if (resetData.ResetBudgetGoals)
        {
            var budgetGoals = await _dbContext.BudgetGoals
                .Where(bg => bg.GroupId == groupId)
                .ToListAsync();

            _dbContext.BudgetGoals.RemoveRange(budgetGoals);

            _historyService.AddHistoryRecord(
                groupId: groupId,
                targetUserId: null,
                actionUserId: userId,
                note: Constants.HistoryNotes.GroupBudgetGoalsReset
            );
        }

        if (resetData.ResetReceiptsProductsAndSellers)
        {
            var receipts = await _dbContext.Receipts
                .Where(r => r.GroupId == groupId)
                .ToListAsync();

            var receiptIds = receipts.Select(r => r.Id).ToList();

            var products = await _dbContext.ProductData
                .Where(p => p.GroupId == groupId)
                .ToListAsync();

            var sellers = await _dbContext.Sellers
                .Where(s => s.GroupId == groupId)
                .ToListAsync();

            _dbContext.ProductData.RemoveRange(products);
            _dbContext.Sellers.RemoveRange(sellers);
            _dbContext.Receipts.RemoveRange(receipts);

            _historyService.AddHistoryRecord(
                groupId: groupId,
                targetUserId: null,
                actionUserId: userId,
                note: Constants.HistoryNotes.GroupReceiptsProductsAndSellersReset
            );
        }

        return Map(group);
    }

    public async Task DeleteGroupAsync(int groupId)
    {
        var group = await _dbContext.Groups
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
            throw new InvalidOperationException(Constants.ErrorMessages.GroupNotFound);

        _dbContext.Groups.Remove(group);
        await _dbContext.SaveChangesAsync();
    }

    public static int CalculateMaxMembers(Group? group)
    {
        if (group == null) return 0;
        
        return group.IsPersonal ? 1 : Constants.ServiceConstants.MaxMembersPerGroup;
    }

    private static GroupDto Map(Group g) => new GroupDto
    {
        Id = g.Id,
        Name = g.Name,
        IsFull = g.Members.Count(m => m.Active) >= CalculateMaxMembers(g),
        IsPersonal = g.IsPersonal,
        OwnerId = g.OwnerId,
        CreatedDate = g.CreatedDate
    };
}