using TaskFlow.Application.Abstractions.Messaging;

namespace TaskFlow.Application.Organizations.Commands.Invitations.CancelInvitation;

public sealed record CancelInvitationCommand(
    Guid InvitationId) : ICommand<Guid>;