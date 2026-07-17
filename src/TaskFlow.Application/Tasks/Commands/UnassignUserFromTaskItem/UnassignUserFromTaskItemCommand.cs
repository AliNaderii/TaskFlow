using TaskFlow.Application.Abstractions.Messaging;

namespace TaskFlow.Application.Tasks.Commands.UnassignUserFromTaskItem;

public sealed record UnassignUserFromTaskItemCommand(
    Guid TaskItemId)
    : ICommand;