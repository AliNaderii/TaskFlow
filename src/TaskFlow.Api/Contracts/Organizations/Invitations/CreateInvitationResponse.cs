namespace TaskFlow.Api.Contracts.Organizations.Invitations;

public sealed record CreateInvitationResponse(
    Guid Id,
    string Email,
    string Role,
    string Status,
    DateTime ExpiresAt,
    string Token);