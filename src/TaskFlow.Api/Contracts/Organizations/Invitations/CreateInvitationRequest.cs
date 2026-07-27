using TaskFlow.Domain.Enums;

namespace TaskFlow.Api.Contracts.Organizations.Invitations;

public sealed record CreateInvitationRequest(
    string Email,
    MembershipRole Role,
    int ExpirationDays = 7);