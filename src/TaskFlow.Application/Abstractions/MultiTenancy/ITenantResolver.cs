namespace TaskFlow.Application.Abstractions.MultiTenancy;

public interface ITenantResolver
{
    Task<Guid?> ResolveAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}