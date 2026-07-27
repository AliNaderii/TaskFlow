using TaskFlow.Application.Abstractions.Authentication;
using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.MultiTenancy;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Organizations.Commands.Invitations.CancelInvitation;

public sealed class CancelInvitationCommandHandler
    : ICommandHandler<CancelInvitationCommand, Guid>
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    public CancelInvitationCommandHandler(
        IInvitationRepository invitationRepository,
        IMembershipRepository membershipRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _invitationRepository = invitationRepository;
        _membershipRepository = membershipRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(
        CancelInvitationCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentTenant.OrganizationId.HasValue)
        {
            return Result<Guid>.Failure(TenantErrors.NotFound);
        }

        var organizationId = _currentTenant.OrganizationId.Value;

        if (!_currentUser.Id.HasValue)
        {
            return Result<Guid>.Failure(UserErrors.NotFound);
        }

        var currentUserId = _currentUser.Id.Value;

        var isAdmin = await _membershipRepository.IsUserAdminAsync(
            currentUserId,
            organizationId,
            cancellationToken);

        if (!isAdmin)
        {
            return Result<Guid>.Failure(AuthorizationErrors.Forbidden);
        }

        var invitation = await _invitationRepository.GetByIdAsync(
            request.InvitationId,
            cancellationToken);

        if (invitation is null)
        {
            return Result<Guid>.Failure(InvitationErrors.NotFound);
        }

        if (invitation.OrganizationId != organizationId)
        {
            return Result<Guid>.Failure(InvitationErrors.NotFound);
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            return Result<Guid>.Failure(InvitationErrors.NotPending);
        }

        var cancelResult = invitation.Cancel();

        if (cancelResult.IsFailure)
        {
            return Result<Guid>.Failure(cancelResult.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(invitation.Id);
    }
}