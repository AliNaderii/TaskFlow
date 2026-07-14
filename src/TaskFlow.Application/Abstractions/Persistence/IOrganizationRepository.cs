using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Abstractions.Persistence;

public interface IOrganizationRepository
{
    Task<bool> ExistsByNameAsync(
        OrganizationName name,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Organization organization,
        CancellationToken cancellationToken = default);
    
    Task<Organization?> GetByIdAsync(
        Guid Id,
        CancellationToken cancellationToken);
}