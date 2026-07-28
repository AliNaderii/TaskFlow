using TaskFlow.Domain.Common;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Errors;

namespace TaskFlow.Domain.Entities;

public class Membership : AuditableEntity, ITenantEntity
{
    public Guid UserId { get; private set; }
    public Guid OrganizationId { get; private set; }
    public MembershipRole Role { get; private set; }
    public MembershipStatus Status { get; private set; }
    public User User { get; private set; } = null!;
    public Organization Organization { get; private set; } = null!;

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

    public BaseResult Remove()
    {
        if (Role == MembershipRole.Owner)
        {
            return BaseResult.Failure(MembershipErrors.CannotRemoveOwner);
        }

        if (Status == MembershipStatus.Removed)
        {
            return BaseResult.Success();
        }

        Status = MembershipStatus.Removed;
        return BaseResult.Success();
    }

    public BaseResult Leave()
    {
        if (Role == MembershipRole.Owner)
        {
            return BaseResult.Failure(MembershipErrors.CannotLeaveAsOwner);
        }

        if (Status == MembershipStatus.Removed || Status == MembershipStatus.Left)
        {
            return BaseResult.Success();
        }

        Status = MembershipStatus.Left;
        return BaseResult.Success();
    }

    public BaseResult ChangeRole(MembershipRole newRole)
    {
        if (Role == MembershipRole.Owner)
        {
            return BaseResult.Failure(MembershipErrors.CannotChangeOwnerRole);
        }

        if (newRole == MembershipRole.Owner)
        {
            return BaseResult.Failure(MembershipErrors.CannotDemoteOwner);
        }

        if (Role == newRole)
        {
            return BaseResult.Success();
        }

        Role = newRole;
        return BaseResult.Success();
    }

    public BaseResult Suspend()
    {
        if (Role == MembershipRole.Owner)
        {
            return BaseResult.Failure(MembershipErrors.CannotChangeOwnerRole);
        }

        if (Status == MembershipStatus.Suspended)
        {
            return BaseResult.Failure(MembershipErrors.AlreadySuspended);
        }

        Status = MembershipStatus.Suspended;
        return BaseResult.Success();
    }

    public BaseResult Activate()
    {
        if (Status != MembershipStatus.Suspended)
        {
            return BaseResult.Failure(MembershipErrors.NotSuspended);
        }

        Status = MembershipStatus.Active;
        return BaseResult.Success();
    }

    public BaseResult TransferOwnership(Membership targetMembership)
    {
        if (Role != MembershipRole.Owner)
        {
            return BaseResult.Failure(MembershipErrors.OnlyOwnerCanTransferOwnership);
        }

        if (targetMembership == null)
        {
            return BaseResult.Failure(MembershipErrors.TargetMemberNotFound);
        }

        if (targetMembership.UserId == UserId)
        {
            return BaseResult.Failure(MembershipErrors.CannotTransferOwnershipToSelf);
        }

        if (targetMembership.Role == MembershipRole.Owner)
        {
            return BaseResult.Failure(MembershipErrors.TargetMemberAlreadyOwner);
        }

        if (targetMembership.Status != MembershipStatus.Active)
        {
            return BaseResult.Failure(MembershipErrors.TargetMemberNotActive);
        }

        // Current owner becomes Admin
        Role = MembershipRole.Admin;

        // Target member becomes Owner
        targetMembership.Role = MembershipRole.Owner;

        return BaseResult.Success();
    }
}
