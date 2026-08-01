using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Abstractions.Persistence;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Project>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Project project,
        CancellationToken cancellationToken = default);
    
    Task<bool> ExistsByNameAsync(
        ProjectName name,
        Guid? excludedProjectId = null,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Project> Items, int TotalCount)> SearchAsync(
        string? keyword,
        bool? isArchived,
        int page,
        int pageSize,
        string? sortBy,
        string? sortDirection,
        CancellationToken cancellationToken = default);
}
