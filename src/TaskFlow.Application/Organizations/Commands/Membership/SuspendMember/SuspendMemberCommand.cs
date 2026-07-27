using TaskFlow.Application.Abstractions.Messaging;

namespace TaskFlow.Application.Organizations.Commands.Membership.SuspendMember;

public sealed record SuspendMemberCommand(Guid UserId) : ICommand;