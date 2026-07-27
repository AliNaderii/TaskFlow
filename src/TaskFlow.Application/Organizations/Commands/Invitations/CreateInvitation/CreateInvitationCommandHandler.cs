using TaskFlow.Application.Abstractions.Authentication;
using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.MultiTenancy;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Organizations.Commands.Invitations.CreateInvitation;

public sealed class CreateInvitationCommandHandler
    : ICommandHandler<CreateInvitationCommand, Guid>
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    public CreateInvitationCommandHandler(
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
        CreateInvitationCommand request,
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

        if (await _invitationRepository.ExistsPendingByEmailAsync(
                request.Email,
                organizationId,
                cancellationToken))
        {
            return Result<Guid>.Failure(InvitationErrors.AlreadyExists);
        }

        var userExists = await _membershipRepository.ExistsByEmailAsync(
            request.Email,
            organizationId,
            cancellationToken);

        if (userExists)
        {
            return Result<Guid>.Failure(InvitationErrors.UserAlreadyMember);
        }

        var expiresAt = DateTime.UtcNow.AddDays(request.ExpirationDays);

        var invitationResult = Invitation.Create(
            organizationId,
            request.Email,
            currentUserId,
            request.Role,
            expiresAt);

        if (invitationResult.IsFailure)
        {
            return Result<Guid>.Failure(invitationResult.Error);
        }

        var invitation = invitationResult.Value;

        await _invitationRepository.AddAsync(invitation, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(invitation.Id);
    }
}
