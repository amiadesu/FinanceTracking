using System;

namespace FinanceTracking.API.Models;

public enum InvitationStatus
{
    Pending,
    Accepted,
    Rejected,
    Cancelled
}

public class GroupInvitation
{
    public Guid Id { get; set; }
    
    public int GroupId { get; set; }
    public Guid InvitedByUserId { get; set; }
    public Guid TargetUserId { get; set; } 
    
    public string Note { get; set; }
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public DateTime CreatedDate { get; set; }

    public Group Group { get; set; }
    public AppUser InvitedByUser { get; set; }
    public AppUser TargetUser { get; set; }
}