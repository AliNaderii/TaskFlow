using TaskFlow.Application.Abstractions.Authentication;
using TaskFlow.Application.Abstractions.MultiTenancy;

namespace TaskFlow.Infrastructure.MultiTenancy;

public sealed class TenantContextInitializer : ITenantContextInitializer
{
    private readonly ICurrentUser _currentUser;
    private readonly ITenantResolver _tenantResolver;
    private readonly CurrentTenant _currentTenant;

    public TenantContextInitializer(
        ICurrentUser currentUser,
        ITenantResolver tenantResolver,
        CurrentTenant currentTenant)
    {
        _currentUser = currentUser;
        _tenantResolver = tenantResolver;
        _currentTenant = currentTenant;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId is null)
            return;

        var organizationId = await _tenantResolver.ResolveAsync(
            _currentUser.UserId.Value,
            cancellationToken);

        if (organizationId.HasValue)
        {
            _currentTenant.SetTenant(
                organizationId.Value);
        }
    }
}