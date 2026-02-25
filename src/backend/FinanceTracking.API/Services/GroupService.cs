using System;
using System.Threading.Tasks;
using FinanceTracking.API.Data;
using FinanceTracking.API.Models;

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
}