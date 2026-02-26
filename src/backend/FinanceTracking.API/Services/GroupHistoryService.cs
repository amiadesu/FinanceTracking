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
        bool? activeAfter = null)
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
            ChangedAt = DateTime.UtcNow
        };

        _dbContext.GroupMemberHistories.Add(historyEntry);
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