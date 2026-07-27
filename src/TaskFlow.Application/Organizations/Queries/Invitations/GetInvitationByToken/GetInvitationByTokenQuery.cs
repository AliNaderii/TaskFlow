using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Organizations.Queries.Invitations.GetInvitationByToken;

public sealed record GetInvitationByTokenQuery(
    string Token) : IQuery<InvitationResponse>;

public sealed record InvitationResponse(
    Guid Id,
    Guid OrganizationId,
    string OrganizationName,
    string Email,
    string Role,
    string Token,
    string Status,
    DateTime ExpiresAt,
    DateTime CreatedAt);