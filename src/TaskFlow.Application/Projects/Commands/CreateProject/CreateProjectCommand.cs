using TaskFlow.Application.Abstractions.Messaging;

namespace TaskFlow.Application.Projects.Commands.CreateProject;

public sealed record CreateProjectCommand(
    Guid OrganizationId,
    string Name,
    string? Description)
    : ICommand<Guid>;