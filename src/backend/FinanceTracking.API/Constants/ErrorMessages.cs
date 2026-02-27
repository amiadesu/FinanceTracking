namespace FinanceTracking.API.Constants;

public static class ErrorMessages
{
    // General
    public const string UserNotFound = "User not found.";
    public const string UnauthorizedAccess = "You do not have permission to perform this action.";

    // Groups
    public const string InvalidGroupId = "Invalid group ID.";
    public const string GroupNotFound = "Group not found.";

    // Invitations
    public const string InvitationNotFound = "Invitation not found.";
    public const string UserAlreadyActiveMember = "User is already an active member of this group.";
    public const string PendingInvitationExists = "A pending invitation already exists for this user.";
    public const string InvitationNotPending = "This action can only be performed on pending invitations.";
    public const string CannotCancelInvitation = "You do not have permission to cancel this invitation.";
    public const string InvitationNotFoundOrUnauthorized = "Invitation not found or you are not authorized to view it.";
}