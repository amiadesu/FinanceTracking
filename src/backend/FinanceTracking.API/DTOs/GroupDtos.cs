using System;
using FinanceTracking.API.Models;

namespace FinanceTracking.API.DTOs;

public record GroupDto(int Id, string Name, bool IsPersonal, Guid? OwnerId, DateTime CreatedDate);

public record GroupMemberDto(Guid UserId, string UserName, GroupRole Role, bool Active, DateTime JoinedDate);

public record GroupHistoryDto(int Id, string Note, GroupRole? RoleIdBefore, GroupRole? RoleIdAfter, 
                              bool? ActiveBefore, bool? ActiveAfter, DateTime ChangedAt, 
                              string TargetUserName, string ChangedByUserName);