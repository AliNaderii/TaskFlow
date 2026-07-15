using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Abstractions.Persistence;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Project>> GetByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Project project,
        CancellationToken cancellationToken = default);
    
    Task<bool> ExistsByNameAsync(
        Guid organizationId,
        ProjectName name,
        Guid? excludedProjectId = null,
        CancellationToken cancellationToken = default);
}