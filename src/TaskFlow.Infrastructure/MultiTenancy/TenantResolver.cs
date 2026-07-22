using TaskFlow.Application.Abstractions.MultiTenancy;
using TaskFlow.Application.Abstractions.Persistence;

namespace TaskFlow.Infrastructure.MultiTenancy;

public sealed class TenantResolver : ITenantResolver
{
    private readonly IMembershipRepository _membershipRepository;

    public TenantResolver(
        IMembershipRepository membershipRepository)
    {
        _membershipRepository = membershipRepository;
    }

    public async Task<Guid?> ResolveAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _membershipRepository.GetOrganizationIdForUserAsync(
            userId,
            cancellationToken);
    }
}