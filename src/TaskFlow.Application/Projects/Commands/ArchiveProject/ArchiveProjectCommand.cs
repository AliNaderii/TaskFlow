using TaskFlow.Application.Abstractions.Messaging;

namespace TaskFlow.Application.Projects.Commands.ArchiveProject;

public sealed record ArchiveProjectCommand(Guid Id) : ICommand;