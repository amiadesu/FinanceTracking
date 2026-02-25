using System;
using System.Collections.Generic;

namespace FinanceTracking.API.Models;

public enum GroupRole
{
    Owner = 1,
    Admin = 2,
    Member = 3
}

public class Role
{
    public GroupRole Id { get; set; }
    public string Name { get; set; }

    public ICollection<GroupMember> GroupMembers { get; set; }
}