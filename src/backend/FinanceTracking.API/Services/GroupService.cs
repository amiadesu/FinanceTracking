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
            .Where(g => g.Members.Any(m => m.UserId == userId && m.Active))
            .Select(g => Map(g))
            .ToListAsync();
    }

    public async Task<GroupDto?> GetGroupByIdAsync(int groupId)
    {
        var group = await _dbContext.Groups
            .FirstOrDefaultAsync(g => g.Id == groupId);
        return group == null ? null : Map(group);
    }

    private static GroupDto Map(Group g) => new GroupDto
    {
        Id = g.Id,
        Name = g.Name,
        IsPersonal = g.IsPersonal,
        OwnerId = g.OwnerId,
        CreatedDate = g.CreatedDate
    };
}