using TaskFlow.Application.Abstractions.Messaging;

namespace TaskFlow.Application.Organizations.Commands.Membership.ActivateMember;

public sealed record ActivateMemberCommand(Guid UserId) : ICommand;