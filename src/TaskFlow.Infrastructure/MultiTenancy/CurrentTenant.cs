using TaskFlow.Application.Abstractions.MultiTenancy;

namespace TaskFlow.Infrastructure.MultiTenancy;

public sealed class CurrentTenant : ICurrentTenant
{
    public Guid? OrganizationId { get; private set; }

    public void SetTenant(Guid organizationId)
    {
        OrganizationId = organizationId;
    }
}