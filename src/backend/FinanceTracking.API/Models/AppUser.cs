using System;
using System.Collections.Generic;

namespace FinanceTracking.API.Models;

public class AppUser
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }

    public ICollection<Group> OwnedGroups { get; set; }
    public ICollection<GroupMember> GroupMemberships { get; set; }
    public ICollection<GroupMemberHistory> HistoryActions { get; set; }
    public ICollection<GroupMemberHistory> HistoryChangesMade { get; set; }
    public ICollection<Receipt> CreatedReceipts { get; set; }
}