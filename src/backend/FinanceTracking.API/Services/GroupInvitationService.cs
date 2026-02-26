using System;
using System.Collections.Generic;
using System.Linq;
using FinanceTracking.API.Data;
using FinanceTracking.API.Models;
using FinanceTracking.API.DTOs;
using FinanceTracking.API.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracking.API.Services;

public class GroupInvitationService
{
    private readonly FinanceDbContext _context;
    private readonly GroupService _groupService;
    private readonly GroupHistoryService _historyService;

    public GroupInvitationService(
        FinanceDbContext context, 
        GroupService groupService, 
        GroupHistoryService historyService)
    {
        _context = context;
        _groupService = groupService;
        _historyService = historyService;
    }

    public async Task<InvitationResponseDto> CreateInvitationAsync(int groupId, Guid currentUserId, CreateInvitationDto dto)
    {
        var targetUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.TargetUserIdentifier || u.UserName == dto.TargetUserIdentifier);

        if (targetUser == null)
            throw new NotFoundException(Constants.ErrorMessages.UserNotFound);

        var isAlreadyMember = await _context.GroupMembers
            .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == targetUser.Id && gm.Active);
            
        if (isAlreadyMember)
            throw new ConflictException(Constants.ErrorMessages.UserAlreadyActiveMember);

        var existingInvite = await _context.GroupInvitations
            .AnyAsync(i => i.GroupId == groupId && i.TargetUserId == targetUser.Id && i.Status == InvitationStatus.Pending);

        if (existingInvite)
            throw new ConflictException(Constants.ErrorMessages.PendingInvitationExists);

        var invitation = new GroupInvitation
        {
            GroupId = groupId,
            InvitedByUserId = currentUserId,
            TargetUserId = targetUser.Id,
            Note = dto.Note,
            Status = InvitationStatus.Pending,
            CreatedDate = DateTime.UtcNow
        };

        _context.GroupInvitations.Add(invitation);

        _historyService.AddHistoryRecord(
            groupId: groupId,
            targetUserId: targetUser.Id,
            actionUserId: currentUserId,
            note: Constants.HistoryNotes.InvitationSent
        );

        await _context.SaveChangesAsync();

        return await GetInvitationAsync(invitation.Id, targetUser.Id);
    }

    public async Task CancelInvitationAsync(int groupId, Guid invitationId, Guid currentUserId)
    {
        var invitation = await _context.GroupInvitations
            .FirstOrDefaultAsync(i => i.Id == invitationId && i.GroupId == groupId);

        if (invitation == null) 
            throw new NotFoundException(Constants.ErrorMessages.InvitationNotFound);
            
        if (invitation.Status != InvitationStatus.Pending)
            throw new BadRequestException(Constants.ErrorMessages.InvitationNotPending);

        var currentUserRole = await _groupService.GetUserRoleInGroupAsync(groupId, currentUserId);
        bool canCancel = invitation.InvitedByUserId == currentUserId || 
                         currentUserRole == GroupRole.Admin || 
                         currentUserRole == GroupRole.Owner;

        if (!canCancel)
            throw new ForbiddenException(Constants.ErrorMessages.CannotCancelInvitation);

        invitation.Status = InvitationStatus.Cancelled;

        _historyService.AddHistoryRecord(
            groupId: groupId,
            targetUserId: invitation.TargetUserId,
            actionUserId: currentUserId,
            note: Constants.HistoryNotes.InvitationCancelled
        );

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<InvitationResponseDto>> GetPendingInvitationsAsync(Guid currentUserId)
    {
        return await _context.GroupInvitations
            .Include(i => i.Group)
            .Include(i => i.InvitedByUser)
            .Include(i => i.TargetUser)
            .Where(i => i.TargetUserId == currentUserId && i.Status == InvitationStatus.Pending)
            .Select(i => new InvitationResponseDto
            {
                Id = i.Id,
                GroupId = i.GroupId,
                GroupName = i.Group.Name,
                InvitedByUserId = i.InvitedByUserId,
                InvitedByUserName = i.InvitedByUser.UserName,
                TargetUserId = i.TargetUserId,
                TargetUserName = i.TargetUser.UserName,
                Note = i.Note,
                Status = i.Status.ToString(),
                CreatedDate = i.CreatedDate
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<InvitationResponseDto>> GetGroupInvitationsAsync(int groupId)
{
    return await _context.GroupInvitations
        .Include(i => i.Group)
        .Include(i => i.InvitedByUser)
        .Include(i => i.TargetUser)
        .Where(i => i.GroupId == groupId)
        .OrderByDescending(i => i.CreatedDate)
        .Select(i => new InvitationResponseDto
        {
            Id = i.Id,
            GroupId = i.GroupId,
            GroupName = i.Group.Name,
            InvitedByUserId = i.InvitedByUserId,
            InvitedByUserName = i.InvitedByUser.UserName,
            TargetUserId = i.TargetUserId,
            TargetUserName = i.TargetUser.UserName,
            Note = i.Note,
            Status = i.Status.ToString(),
            CreatedDate = i.CreatedDate
        })
        .ToListAsync();
}

    public async Task<InvitationResponseDto> GetInvitationAsync(Guid invitationId, Guid currentUserId)
    {
        var invite = await _context.GroupInvitations
            .Include(i => i.Group)
            .Include(i => i.InvitedByUser)
            .Include(i => i.TargetUser)
            .FirstOrDefaultAsync(i => i.Id == invitationId);

        if (invite == null) 
            throw new NotFoundException(Constants.ErrorMessages.InvitationNotFound);

        return new InvitationResponseDto
        {
            Id = invite.Id,
            GroupId = invite.GroupId,
            GroupName = invite.Group.Name,
            InvitedByUserId = invite.InvitedByUserId,
            InvitedByUserName = invite.InvitedByUser.UserName,
            TargetUserId = invite.TargetUserId,
            TargetUserName = invite.TargetUser.UserName,
            Note = invite.Note,
            Status = invite.Status.ToString(),
            CreatedDate = invite.CreatedDate
        };
    }

    public async Task AcceptInvitationAsync(Guid invitationId, Guid currentUserId)
    {
        var invitation = await _context.GroupInvitations.FindAsync(invitationId);

        if (invitation == null || invitation.TargetUserId != currentUserId)
            throw new NotFoundException(Constants.ErrorMessages.InvitationNotFoundOrUnauthorized);

        if (invitation.Status != InvitationStatus.Pending)
            throw new BadRequestException(Constants.ErrorMessages.InvitationNotPending);

        invitation.Status = InvitationStatus.Accepted;

        var newMember = new GroupMember
        {
            GroupId = invitation.GroupId,
            UserId = currentUserId,
            RoleId = GroupRole.Member,
            Active = true,
            JoinedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };

        _context.GroupMembers.Add(newMember);

        _historyService.AddHistoryRecord(
            groupId: invitation.GroupId,
            targetUserId: currentUserId,
            actionUserId: currentUserId, // User accepted it themselves
            note: Constants.HistoryNotes.InvitationAccepted,
            roleAfter: GroupRole.Member,
            activeBefore: false,
            activeAfter: true
        );

        await _context.SaveChangesAsync();
    }

    public async Task RejectInvitationAsync(Guid invitationId, Guid currentUserId)
    {
        var invitation = await _context.GroupInvitations.FindAsync(invitationId);

        if (invitation == null || invitation.TargetUserId != currentUserId)
            throw new NotFoundException(Constants.ErrorMessages.InvitationNotFoundOrUnauthorized);

        if (invitation.Status != InvitationStatus.Pending)
            throw new BadRequestException(Constants.ErrorMessages.InvitationNotPending);

        invitation.Status = InvitationStatus.Rejected;

        _historyService.AddHistoryRecord(
            groupId: invitation.GroupId,
            targetUserId: currentUserId,
            actionUserId: currentUserId, // User rejected it themselves
            note: Constants.HistoryNotes.InvitationRejected
        );

        await _context.SaveChangesAsync();
    }
}