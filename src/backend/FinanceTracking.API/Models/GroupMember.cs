using System;

namespace FinanceTracking.API.Models;

public class GroupMember
{
    public Guid UserId { get; set; }
    public int GroupId { get; set; }
    public GroupRole RoleId { get; set; }
    public bool Active { get; set; }
    public DateTime JoinedDate { get; set; }
    public DateTime UpdatedDate { get; set; }

    public AppUser User { get; set; }
    public Group Group { get; set; }
    public Role Role { get; set; }
}