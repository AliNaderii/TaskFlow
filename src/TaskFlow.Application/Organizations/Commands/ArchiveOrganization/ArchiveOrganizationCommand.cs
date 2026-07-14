using TaskFlow.Application.Abstractions.Messaging;

namespace TaskFlow.Application.Organizations.Commands.ArchiveOrganization;

public sealed record ArchiveOrganizationCommand(Guid Id) : ICommand;