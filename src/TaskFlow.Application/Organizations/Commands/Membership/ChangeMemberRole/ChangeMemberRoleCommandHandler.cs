using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.MultiTenancy;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Errors;

namespace TaskFlow.Application.Organizations.Commands.Membership.ChangeMemberRole;

public sealed class ChangeMemberRoleCommandHandler
    : ICommandHandler<ChangeMemberRoleCommand>
{
    private readonly IMembershipRepository _membershipRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;

    public ChangeMemberRoleCommandHandler(
        IMembershipRepository membershipRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant)
    {
        _membershipRepository = membershipRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<BaseResult> Handle(
        ChangeMemberRoleCommand request,
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

        var changeRoleResult = membership.ChangeRole(request.Role);

        if (changeRoleResult.IsFailure)
        {
            return changeRoleResult;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BaseResult.Success();
    }
}