using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Organizations.Queries.Invitations.GetInvitationByToken;

public sealed class GetInvitationByTokenQueryHandler
    : IQueryHandler<GetInvitationByTokenQuery, InvitationResponse>
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly IOrganizationRepository _organizationRepository;

    public GetInvitationByTokenQueryHandler(
        IInvitationRepository invitationRepository,
        IOrganizationRepository organizationRepository)
    {
        _invitationRepository = invitationRepository;
        _organizationRepository = organizationRepository;
    }

    public async Task<Result<InvitationResponse>> Handle(
        GetInvitationByTokenQuery request,
        CancellationToken cancellationToken)
    {
        var tokenResult = InvitationToken.Create(request.Token);

        if (tokenResult.IsFailure)
        {
            return Result<InvitationResponse>.Failure(InvitationErrors.InvalidToken);
        }

        var invitation = await _invitationRepository.GetByTokenAsync(
            tokenResult.Value.Value,
            cancellationToken);

        if (invitation is null)
        {
            return Result<InvitationResponse>.Failure(InvitationErrors.NotFound);
        }

        var organization = await _organizationRepository.GetByIdAsync(
            invitation.OrganizationId,
            cancellationToken);

        var response = new InvitationResponse(
            invitation.Id,
            invitation.OrganizationId,
            organization?.Name.Value ?? string.Empty,
            invitation.Email,
            invitation.Role.ToString(),
            invitation.Token.Value,
            invitation.Status.ToString(),
            invitation.ExpiresAt,
            invitation.CreatedAt);

        return Result<InvitationResponse>.Success(response);
    }
}