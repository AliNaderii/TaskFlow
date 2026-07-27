using TaskFlow.Domain.Enums;

namespace TaskFlow.Api.Contracts.Organizations.Membership;

public sealed record ChangeMemberRoleRequest(MembershipRole Role);