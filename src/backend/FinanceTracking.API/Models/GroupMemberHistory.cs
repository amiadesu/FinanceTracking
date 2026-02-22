using System;

namespace FinanceTracking.API.Models;

public class GroupMemberHistory
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? ChangedByUserId { get; set; }
    public int? RoleIdBefore { get; set; }
    public int? RoleIdAfter { get; set; }
    public bool? ActiveBefore { get; set; }
    public bool? ActiveAfter { get; set; }
    public string Note { get; set; }
    public DateTime ChangedAt { get; set; }

    public Group Group { get; set; }
    public AppUser User { get; set; }
    public AppUser ChangedByUser { get; set; }
}