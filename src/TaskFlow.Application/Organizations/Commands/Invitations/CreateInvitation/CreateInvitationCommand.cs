using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Organizations.Commands.Invitations.CreateInvitation;

public sealed record CreateInvitationCommand(
    string Email,
    MembershipRole Role,
    int ExpirationDays = 7) : ICommand<Guid>;