using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Abstractions.Persistence;

public interface IMembershipRepository
{
    Task<bool> ExistsAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task<Membership?> GetAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Membership membership,
        CancellationToken cancellationToken = default);
}