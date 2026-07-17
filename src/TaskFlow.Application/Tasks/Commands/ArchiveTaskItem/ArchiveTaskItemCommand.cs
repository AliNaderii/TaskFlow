using TaskFlow.Application.Abstractions.Messaging;

namespace TaskFlow.Application.Tasks.Commands.ArchiveTaskItem;

public sealed record ArchiveTaskItemCommand(Guid Id) : ICommand;