using TaskFlow.Domain.Common;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities;

public class Membership : AuditableEntity
{
    public Guid UserId { get; private set; }
    public Guid OrganizationId { get; private set; }
    public MembershipRole Role { get; private set; }
    public MembershipStatus Status { get; private set; }

    private Membership() { }

    private Membership(
        Guid userId,
        Guid organizationId,
        MembershipRole role)
    {
        UserId = userId;
        OrganizationId = organizationId;
        Role = role;
        Status = MembershipStatus.Active;
    }

    public static Result<Membership> Create(
        Guid userId,
        Guid organizationId,
        MembershipRole role)
    {
        if (userId == Guid.Empty)
        {
            return Result<Membership>.Failure(
                new Error(
                    "membership.invalid_user",
                    "User is required."));
        }

        if (organizationId == Guid.Empty)
        {
            return Result<Membership>.Failure(
                new Error(
                    "membership.invalid_organization",
                    "Organization is required."));
        }

        return Result<Membership>.Success(
            new Membership(
                userId,
                organizationId,
                role));
    }
}