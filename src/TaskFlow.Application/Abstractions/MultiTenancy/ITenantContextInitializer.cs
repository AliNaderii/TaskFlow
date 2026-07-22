namespace TaskFlow.Application.Abstractions.MultiTenancy;

public interface ITenantContextInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}