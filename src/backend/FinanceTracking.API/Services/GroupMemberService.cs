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

    public GroupMemberService(FinanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<GroupMemberDto>> GetGroupMembersAsync(int groupId)
    {
        return await _dbContext.GroupMembers
            .Where(m => m.GroupId == groupId)
            .Select(m => Map(m))
            .ToListAsync();
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