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

public class UpdateGroupMemberRoleDto
{
    public GroupRole? Role { get; set; }
}