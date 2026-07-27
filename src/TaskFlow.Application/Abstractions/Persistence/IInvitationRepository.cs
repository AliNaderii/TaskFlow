using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Abstractions.Persistence;

public interface IInvitationRepository
{
    Task<Invitation?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Invitation?> GetByTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsPendingByEmailAsync(
        string email,
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Invitation>> GetByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Invitation invitation,
        CancellationToken cancellationToken = default);

    void Update(Invitation invitation);
}