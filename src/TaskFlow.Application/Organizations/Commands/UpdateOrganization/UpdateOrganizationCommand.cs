using TaskFlow.Application.Abstractions.Messaging;

namespace TaskFlow.Application.Organizations.Commands.UpdateOrganization;

public sealed record UpdateOrganizationCommand(Guid Id, string Name) : ICommand;