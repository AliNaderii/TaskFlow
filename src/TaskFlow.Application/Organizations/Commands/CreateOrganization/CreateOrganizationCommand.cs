using TaskFlow.Application.Abstractions.Messaging;

namespace TaskFlow.Application.Organizations.Commands.CreateOrganization;

public sealed record CreateOrganizationCommand(string Name) : ICommand<Guid>;