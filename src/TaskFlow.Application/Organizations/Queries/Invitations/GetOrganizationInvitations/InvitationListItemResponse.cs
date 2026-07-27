namespace TaskFlow.Application.Organizations.Queries.Invitations.GetOrganizationInvitations;

public sealed record InvitationListItemResponse(
    Guid Id,
    Guid OrganizationId,
    string OrganizationName,
    string Email,
    string Role,
    string Status,
    DateTime ExpiresAt,
    DateTime CreatedAt);