using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

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

    Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> SearchAsync(
        Guid? projectId,
        string? keyword,
        TaskItemStatus? status,
        TaskItemPriority? priority,
        Guid? assigneeUserId,
        DateTime? dueDateFrom,
        DateTime? dueDateTo,
        bool? isArchived,
        int page,
        int pageSize,
        string? sortBy,
        string? sortDirection,
        CancellationToken cancellationToken = default);
}
