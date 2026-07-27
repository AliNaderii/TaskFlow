using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.MultiTenancy;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Errors;

namespace TaskFlow.Application.Organizations.Queries.Invitations.GetOrganizationInvitations;

public sealed class GetOrganizationInvitationsQueryHandler
    : IQueryHandler<GetOrganizationInvitationsQuery, IReadOnlyList<InvitationListItemResponse>>
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly ICurrentTenant _currentTenant;

    public GetOrganizationInvitationsQueryHandler(
        IInvitationRepository invitationRepository,
        IOrganizationRepository organizationRepository,
        ICurrentTenant currentTenant)
    {
        _invitationRepository = invitationRepository;
        _organizationRepository = organizationRepository;
        _currentTenant = currentTenant;
    }

    public async Task<Result<IReadOnlyList<InvitationListItemResponse>>> Handle(
        GetOrganizationInvitationsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentTenant.OrganizationId.HasValue)
        {
            return Result<IReadOnlyList<InvitationListItemResponse>>.Failure(TenantErrors.NotFound);
        }

        var organizationId = _currentTenant.OrganizationId.Value;

        if (organizationId != request.OrganizationId)
        {
            return Result<IReadOnlyList<InvitationListItemResponse>>.Failure(AuthorizationErrors.Forbidden);
        }

        var organization = await _organizationRepository.GetByIdAsync(organizationId, cancellationToken);

        if (organization is null)
        {
            return Result<IReadOnlyList<InvitationListItemResponse>>.Failure(OrganizationErrors.NotFound);
        }

        var invitations = await _invitationRepository.GetByOrganizationIdAsync(
            organizationId,
            cancellationToken);

        var pagedInvitations = invitations
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var response = pagedInvitations.Select(i => new InvitationListItemResponse(
            i.Id,
            i.OrganizationId,
            organization.Name.Value,
            i.Email,
            i.Role.ToString(),
            i.Status.ToString(),
            i.ExpiresAt,
            i.CreatedAt)).ToList();

        return Result<IReadOnlyList<InvitationListItemResponse>>.Success(response);
    }
}
