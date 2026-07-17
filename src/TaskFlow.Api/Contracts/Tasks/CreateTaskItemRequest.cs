using TaskFlow.Domain.Enums;

namespace TaskFlow.Api.Tasks.Contracts;

public sealed record CreateTaskItemRequest(
    Guid ProjectId,
    Guid CreatorUserId,
    string Title,
    string? Description,
    TaskItemPriority Priority,
    DateTime? DueDate,
    Guid? AssigneeUserId);