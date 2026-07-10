using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Abstractions.Persistence;

public interface ITaskItemRepository
{
    Task<TaskItem?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TaskItem>> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TaskItem>> GetAssignedToUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        TaskItem taskItem,
        CancellationToken cancellationToken = default);
}