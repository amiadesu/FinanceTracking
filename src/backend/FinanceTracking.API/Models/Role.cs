using System;
using System.Collections.Generic;

namespace FinanceTracking.API.Models;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; }

    public ICollection<GroupMember> GroupMembers { get; set; }
}