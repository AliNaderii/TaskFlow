using TaskFlow.Application.Abstractions.Authorization;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Infrastructure.Authorization;

public sealed class AuthorizationService : IAppAuthorizationService
{
    private readonly IMembershipRepository _membershipRepository;

    public AuthorizationService(IMembershipRepository membershipRepository)
    {
        _membershipRepository = membershipRepository;
    }

    public async Task<bool> IsMemberAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var membership = await _membershipRepository.GetAsync(
            userId,
            organizationId,
            cancellationToken);

        return membership is not null && membership.Status == MembershipStatus.Active;
    }

    public async Task<bool> IsAdminAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var membership = await _membershipRepository.GetAsync(
            userId,
            organizationId,
            cancellationToken);

        return membership is not null
            && membership.Status == MembershipStatus.Active
            && (membership.Role == MembershipRole.Admin || membership.Role == MembershipRole.Owner);
    }

    public async Task<bool> IsProjectManagerAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var membership = await _membershipRepository.GetAsync(
            userId,
            organizationId,
            cancellationToken);

        return membership is not null
            && membership.Status == MembershipStatus.Active
            && membership.Role == MembershipRole.ProjectManager;
    }
}