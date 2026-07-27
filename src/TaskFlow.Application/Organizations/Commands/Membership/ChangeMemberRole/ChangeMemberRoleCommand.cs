using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Organizations.Commands.Membership.ChangeMemberRole;

public sealed record ChangeMemberRoleCommand(Guid UserId, MembershipRole Role) : ICommand;