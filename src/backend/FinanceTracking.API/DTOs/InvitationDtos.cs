using System;

namespace FinanceTracking.API.DTOs;

public class CreateInvitationDto
{
    public string TargetUserIdentifier { get; set; } 
    public string Note { get; set; }
}

public class InvitationResponseDto
{
    public Guid Id { get; set; }
    public int GroupId { get; set; }
    public string GroupName { get; set; }
    public Guid InvitedByUserId { get; set; }
    public string InvitedByUserName { get; set; }
    public string Note { get; set; }
    public string Status { get; set; }
    public DateTime CreatedDate { get; set; }
}