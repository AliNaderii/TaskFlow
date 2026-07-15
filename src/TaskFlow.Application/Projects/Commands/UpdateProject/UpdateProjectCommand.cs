using TaskFlow.Application.Abstractions.Messaging;

namespace TaskFlow.Application.Projects.Commands.UpdateProject;

public sealed record UpdateProjectCommand(
    Guid Id,
    string Name,
    string? Description)
    : ICommand;