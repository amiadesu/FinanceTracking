using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FinanceTracking.API.Data;
using FinanceTracking.API.Models;
using FinanceTracking.API.DTOs;

namespace FinanceTracking.API.Services;

public class GroupHistoryService
{
    private readonly FinanceDbContext _dbContext;

    public GroupHistoryService(FinanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Records the action but relies on the caller to execute SaveChangesAsync() to ensure atomic transactions.
    /// </summary>
    public void AddHistoryRecord(
        int groupId, 
        Guid? targetUserId, 
        Guid actionUserId, 
        string note, 
        GroupRole? roleBefore = null, 
        GroupRole? roleAfter = null, 
        bool? activeBefore = null, 
        bool? activeAfter = null,
        string? nameBefore = null,
        string? nameAfter = null)
    {
        var historyEntry = new GroupMemberHistory
        {
            GroupId = groupId,
            UserId = targetUserId,
            ChangedByUserId = actionUserId,
            Note = note,
            RoleIdBefore = roleBefore,
            RoleIdAfter = roleAfter,
            ActiveBefore = activeBefore,
            ActiveAfter = activeAfter,
            NameBefore = nameBefore,
            NameAfter = nameAfter,
            ChangedAt = DateTime.UtcNow
        };

        _dbContext.GroupMemberHistories.Add(historyEntry);
    }

    public async Task<List<GroupHistoryDto>> GetGroupHistoryAsync(int groupId)
    {
        return await _dbContext.GroupMemberHistories
            .Include(h => h.User)
            .Include(h => h.ChangedByUser)
            .Where(h => h.GroupId == groupId)
            .OrderByDescending(h => h.ChangedAt)
            .Select(h => Map(h))
            .ToListAsync();
    }

    private static GroupHistoryDto Map(GroupMemberHistory h) => new GroupHistoryDto
    {
        Id = h.Id,
        Note = h.Note,
        RoleIdBefore = h.RoleIdBefore,
        RoleIdAfter = h.RoleIdAfter,
        ActiveBefore = h.ActiveBefore,
        ActiveAfter = h.ActiveAfter,
        NameBefore = h.NameBefore,
        NameAfter = h.NameAfter,
        ChangedAt = h.ChangedAt,
        TargetUserName = h.User != null ? h.User.UserName : "Unknown",
        ChangedByUserName = h.ChangedByUser != null ? h.ChangedByUser.UserName : "System"
    };
}