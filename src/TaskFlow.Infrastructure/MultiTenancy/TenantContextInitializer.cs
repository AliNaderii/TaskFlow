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
        System.Console.WriteLine($"[DEBUG] TenantContextInitializer.InitializeAsync called");
        System.Console.WriteLine($"[DEBUG] CurrentUser.Id: {_currentUser.Id}");
        
        if (_currentUser.Id is null)
        {
            System.Console.WriteLine($"[DEBUG] CurrentUser.Id is null, returning early");
            return;
        }

        try
        {
            var organizationId = await _tenantResolver.ResolveAsync(
                _currentUser.Id.Value,
                cancellationToken);

            System.Console.WriteLine($"[DEBUG] Resolved organizationId: {organizationId}");

            if (organizationId.HasValue)
            {
                _currentTenant.SetTenant(organizationId.Value);
                System.Console.WriteLine($"[DEBUG] Set tenant to: {organizationId.Value}");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[DEBUG] Exception in TenantContextInitializer: {ex}");
            throw;
        }
    }
}