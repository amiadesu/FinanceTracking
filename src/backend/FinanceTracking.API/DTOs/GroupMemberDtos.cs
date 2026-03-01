using System;
using FinanceTracking.API.Models;

namespace FinanceTracking.API.DTOs;

public class GroupMemberDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public GroupRole Role { get; set; }
    public bool Active { get; set; }
    public DateTime JoinedDate { get; set; }
}

public class GroupMemberListResponseDto
{
    public int CurrentCount { get; set; }
    public int MaxAllowed { get; set; }
    
    public List<GroupMemberDto> GroupMembers { get; set; } = new();
}

public class UpdateGroupMemberRoleDto
{
    public GroupRole? Role { get; set; }
}