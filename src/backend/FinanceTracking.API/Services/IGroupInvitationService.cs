using FinanceTracking.API.Models;
using FinanceTracking.API.DTOs;

namespace FinanceTracking.API.Services;

public interface IGroupInvitationService
{
    Task<InvitationResponseDto> CreateInvitationAsync(int groupId, Guid currentUserId, CreateInvitationDto dto);
    Task CancelInvitationAsync(int groupId, Guid invitationId, Guid currentUserId);
    Task<IEnumerable<InvitationResponseDto>> GetPendingInvitationsAsync(Guid currentUserId);
    Task<int> GetAvailablePendingInvitationCountAsync(Guid currentUserId);
    Task<IEnumerable<InvitationResponseDto>> GetGroupInvitationsAsync(int groupId);
    Task<InvitationResponseDto> GetInvitationAsync(Guid invitationId, Guid currentUserId);
    Task AcceptInvitationAsync(Guid invitationId, Guid currentUserId);
    Task RejectInvitationAsync(Guid invitationId, Guid currentUserId);
}