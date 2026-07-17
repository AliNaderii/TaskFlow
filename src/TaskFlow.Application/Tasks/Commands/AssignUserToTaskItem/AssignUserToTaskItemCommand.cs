using TaskFlow.Application.Abstractions.Messaging;

namespace TaskFlow.Application.Tasks.Commands.AssignUserToTaskItem;

public sealed record AssignUserToTaskItemCommand(
    Guid TaskItemId,
    Guid AssigneeUserId)
    : ICommand;