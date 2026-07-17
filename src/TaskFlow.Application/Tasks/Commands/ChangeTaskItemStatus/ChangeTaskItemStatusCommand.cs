using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Tasks.Commands.ChangeTaskItemStatus;

public sealed record ChangeTaskItemStatusCommand(
    Guid TaskItemId,
    TaskItemStatus Status)
    : ICommand;