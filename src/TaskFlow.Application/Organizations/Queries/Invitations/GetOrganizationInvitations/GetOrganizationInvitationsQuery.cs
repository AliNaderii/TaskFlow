using TaskFlow.Application.Abstractions.Messaging;

namespace TaskFlow.Application.Organizations.Queries.Invitations.GetOrganizationInvitations;

public sealed record GetOrganizationInvitationsQuery(
    Guid OrganizationId,
    int Page = 1,
    int PageSize = 20) : IQuery<IReadOnlyList<InvitationListItemResponse>>;