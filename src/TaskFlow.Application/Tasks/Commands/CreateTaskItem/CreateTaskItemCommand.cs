using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Tasks.Commands.CreateTaskItem;

public sealed record CreateTaskItemCommand(
    Guid ProjectId,
    string Title,
    string? Description,
    TaskItemPriority Priority,
    DateTime? DueDate,
    Guid? AssigneeUserId)
    : ICommand<Guid>;