using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Tasks.Commands.UpdateTaskItem;

public sealed record UpdateTaskItemCommand(
    Guid Id,
    string Title,
    string? Description,
    TaskItemPriority Priority,
    DateTime? DueDate)
    : ICommand;