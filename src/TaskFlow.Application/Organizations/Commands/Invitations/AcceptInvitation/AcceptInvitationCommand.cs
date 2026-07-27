using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Organizations.Commands.Invitations.AcceptInvitation;

public sealed record AcceptInvitationCommand(
    string Token) : ICommand<Guid>;