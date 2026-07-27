using TaskFlow.Application.Abstractions.Authentication;
using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.MultiTenancy;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Organizations.Commands.Invitations.AcceptInvitation;

public sealed class AcceptInvitationCommandHandler
    : ICommandHandler<AcceptInvitationCommand, Guid>
{
    private readonly IInvitationRepository _invitationRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    public AcceptInvitationCommandHandler(
        IInvitationRepository invitationRepository,
        IMembershipRepository membershipRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _invitationRepository = invitationRepository;
        _membershipRepository = membershipRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(
        AcceptInvitationCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.Id.HasValue)
        {
            return Result<Guid>.Failure(UserErrors.NotFound);
        }

        var currentUserId = _currentUser.Id.Value;

        var tokenResult = InvitationToken.Create(request.Token);

        if (tokenResult.IsFailure)
        {
            return Result<Guid>.Failure(InvitationErrors.InvalidToken);
        }

        var invitation = await _invitationRepository.GetByTokenAsync(
            tokenResult.Value.Value,
            cancellationToken);

        if (invitation is null)
        {
            return Result<Guid>.Failure(InvitationErrors.NotFound);
        }

        if (!invitation.IsValid())
        {
            return Result<Guid>.Failure(InvitationErrors.Expired);
        }

        var emailResult = Email.Create(invitation.Email);

        if (emailResult.IsFailure)
        {
            return Result<Guid>.Failure(InvitationErrors.InvalidEmail);
        }

        var user = await _userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);

        if (user is null)
        {
            return Result<Guid>.Failure(InvitationErrors.UserNotRegistered);
        }

        if (user.Id != currentUserId)
        {
            return Result<Guid>.Failure(InvitationErrors.InvalidUser);
        }

        var alreadyMember = await _membershipRepository.ExistsAsync(
            currentUserId,
            invitation.OrganizationId,
            cancellationToken);

        if (alreadyMember)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Guid>.Failure(InvitationErrors.UserAlreadyMember);
        }

        var membershipResult = TaskFlow.Domain.Entities.Membership.Create(
            currentUserId,
            invitation.OrganizationId,
            invitation.Role);

        if (membershipResult.IsFailure)
        {
            return Result<Guid>.Failure(membershipResult.Error);
        }

        var acceptResult = invitation.Accept(currentUserId);

        if (acceptResult.IsFailure)
        {
            return Result<Guid>.Failure(acceptResult.Error);
        }

        await _membershipRepository.AddAsync(membershipResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(invitation.Id);
    }
}
