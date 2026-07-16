using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Tasks.Queries.GetTaskItemById;

public sealed record TaskItemDto(
    Guid Id,
    Guid ProjectId,
    Guid CreatorUserId,
    Guid? AssigneeUserId,
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskItemPriority Priority,
    DateTime? DueDate,
    bool IsArchived,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? ArchivedAt);