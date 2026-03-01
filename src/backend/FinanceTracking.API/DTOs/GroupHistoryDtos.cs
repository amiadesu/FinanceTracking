using System;
using FinanceTracking.API.Models;

namespace FinanceTracking.API.DTOs;

public class GroupHistoryDto
{
    public int Id { get; set; }
    public string Note { get; set; } = string.Empty;
    public GroupRole? RoleIdBefore { get; set; }
    public GroupRole? RoleIdAfter { get; set; }
    public bool? ActiveBefore { get; set; }
    public bool? ActiveAfter { get; set; }
    public DateTime ChangedAt { get; set; }
    public string TargetUserName { get; set; } = string.Empty;
    public string ChangedByUserName { get; set; } = string.Empty;
}