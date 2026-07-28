using TaskFlow.Application.Abstractions.Messaging;

namespace TaskFlow.Application.Organizations.Commands.Membership.TransferOwnership;

public sealed record TransferOwnershipCommand(Guid TargetUserId) : ICommand;
