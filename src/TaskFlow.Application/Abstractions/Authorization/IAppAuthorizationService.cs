namespace TaskFlow.Application.Abstractions.Authorization;

public interface IAppAuthorizationService
{
    Task<bool> IsMemberAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task<bool> IsAdminAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task<bool> IsProjectManagerAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default);
}