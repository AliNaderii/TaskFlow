using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.MultiTenancy;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Errors;

namespace TaskFlow.Application.Organizations.Commands.Membership.SuspendMember;

public sealed class SuspendMemberCommandHandler
    : ICommandHandler<SuspendMemberCommand>
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;

    public SuspendMemberCommandHandler(
        IMembershipRepository membershipRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant)
    {
        _membershipRepository = membershipRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<BaseResult> Handle(
        SuspendMemberCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentTenant.OrganizationId.HasValue)
        {
            return BaseResult.Failure(TenantErrors.NotFound);
        }

        var organizationId = _currentTenant.OrganizationId.Value;

        var membership = await _membershipRepository.GetByUserIdAsync(
            request.UserId,
            organizationId,
            cancellationToken);

        if (membership is null)
        {
            return BaseResult.Failure(MembershipErrors.MemberNotFound);
        }

        var suspendResult = membership.Suspend();

        if (suspendResult.IsFailure)
        {
            return suspendResult;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BaseResult.Success();
    }
}