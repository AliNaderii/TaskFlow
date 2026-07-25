using TaskFlow.Application.Abstractions.Messaging;

namespace TaskFlow.Application.Projects.Commands.CreateProject;

public sealed record CreateProjectCommand(
    string Name,
    string? Description)
    : ICommand<Guid>;