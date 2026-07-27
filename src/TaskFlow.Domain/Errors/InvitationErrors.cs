using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Errors;

public static class InvitationErrors
{
    public static readonly Error NotFound = new("invitation.not_found", "Invitation not found.");
    public static readonly Error AlreadyAccepted = new("invitation.already_accepted", "Invitation has already been accepted.");
    public static readonly Error Expired = new("invitation.expired", "Invitation has expired.");
    public static readonly Error InvalidToken = new("invitation.invalid_token", "Invalid invitation token.");
    public static readonly Error NotPending = new("invitation.not_pending", "Invitation is not pending.");
    public static readonly Error InvalidOrganization = new("invitation.invalid_organization", "Invalid organization.");
    public static readonly Error InvalidEmail = new("invitation.invalid_email", "Invalid email address.");
    public static readonly Error InvalidInvitedByUser = new("invitation.invalid_invited_by_user", "Invalid invited by user.");
    public static readonly Error InvalidExpiration = new("invitation.invalid_expiration", "Expiration must be in the future.");
    public static readonly Error UserAlreadyMember = new("invitation.user_already_member", "User is already a member of this organization.");
    public static readonly Error Cancelled = new("invitation.cancelled", "Invitation has been cancelled.");
    public static readonly Error UserNotRegistered = new("invitation.user_not_registered", "User is not registered.");
    public static readonly Error InvalidUser = new("invitation.invalid_user", "Invalid user for this invitation.");
    public static readonly Error AlreadyExists = new("invitation.already_exists", "A pending invitation already exists for this email.");
}
