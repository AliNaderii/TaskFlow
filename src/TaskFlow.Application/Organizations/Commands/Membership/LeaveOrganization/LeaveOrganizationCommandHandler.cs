using TaskFlow.Application.Abstractions.Authentication;
using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.MultiTenancy;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Errors;

namespace TaskFlow.Application.Organizations.Commands.Membership.LeaveOrganization;

public sealed class LeaveOrganizationCommandHandler
    : ICommandHandler<LeaveOrganizationCommand>
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    public LeaveOrganizationCommandHandler(
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
        LeaveOrganizationCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentTenant.OrganizationId.HasValue)
        {
            return BaseResult.Failure(TenantErrors.NotFound);
        }

        if (!_currentUser.Id.HasValue)
        {
            return BaseResult.Failure(UserErrors.NotFound);
        }

        var organizationId = _currentTenant.OrganizationId.Value;
        var userId = _currentUser.Id.Value;

        var membership = await _membershipRepository.GetByUserIdAsync(
            userId,
            organizationId,
            cancellationToken);

        if (membership is null)
        {
            return BaseResult.Failure(MembershipErrors.MemberNotFound);
        }

        var leaveResult = membership.Leave();

        if (leaveResult.IsFailure)
        {
            return leaveResult;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BaseResult.Success();
    }
}