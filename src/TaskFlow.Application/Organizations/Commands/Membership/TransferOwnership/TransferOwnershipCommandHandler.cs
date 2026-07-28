using TaskFlow.Application.Abstractions.Authentication;
using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.MultiTenancy;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Errors;

namespace TaskFlow.Application.Organizations.Commands.Membership.TransferOwnership;

public sealed class TransferOwnershipCommandHandler
    : ICommandHandler<TransferOwnershipCommand>
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    public TransferOwnershipCommandHandler(
        IMembershipRepository membershipRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _membershipRepository = membershipRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<BaseResult> Handle(
        TransferOwnershipCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentTenant.OrganizationId.HasValue)
        {
            return BaseResult.Failure(TaskFlow.Domain.Errors.TenantErrors.NotFound);
        }

        if (!_currentUser.Id.HasValue)
        {
            return BaseResult.Failure(AuthenticationErrors.UserNotFound);
        }

        var organizationId = _currentTenant.OrganizationId.Value;
        var currentUserId = _currentUser.Id.Value;
        var targetUserId = request.TargetUserId;

        var currentMembership = await _membershipRepository.GetByUserIdAsync(
            currentUserId,
            organizationId,
            cancellationToken);

        if (currentMembership is null)
        {
            return BaseResult.Failure(MembershipErrors.MemberNotFound);
        }

        var targetMembership = await _membershipRepository.GetByUserIdAsync(
            targetUserId,
            organizationId,
            cancellationToken);

        if (targetMembership is null)
        {
            return BaseResult.Failure(MembershipErrors.TargetMemberNotFound);
        }

        var transferResult = currentMembership.TransferOwnership(targetMembership);

        if (transferResult.IsFailure)
        {
            return transferResult;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BaseResult.Success();
    }
}