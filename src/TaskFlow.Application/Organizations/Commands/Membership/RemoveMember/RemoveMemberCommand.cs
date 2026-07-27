using TaskFlow.Application.Abstractions.Messaging;

namespace TaskFlow.Application.Organizations.Commands.Membership.RemoveMember;

public sealed record RemoveMemberCommand(Guid UserId) : ICommand;