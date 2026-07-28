using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Errors;

public static class MembershipErrors
{
    public static readonly Error NotFound =
        new(
            "Membership.NotFound",
            "Membership does not exist.");

    public static readonly Error CannotRemoveOwner =
        new(
            "Membership.CannotRemoveOwner",
            "Cannot remove the organization owner.");

    public static readonly Error CannotLeaveAsOwner =
        new(
            "Membership.CannotLeaveAsOwner",
            "Owner cannot leave the organization. Transfer ownership first.");

    public static readonly Error CannotChangeOwnerRole =
        new(
            "Membership.CannotChangeOwnerRole",
            "Cannot change the owner's role.");

    public static readonly Error CannotDemoteOwner =
        new(
            "Membership.CannotDemoteOwner",
            "Owner role cannot be assigned or removed.");

    public static readonly Error AlreadySuspended =
        new(
            "Membership.AlreadySuspended",
            "Member is already suspended.");

    public static readonly Error NotSuspended =
        new(
            "Membership.NotSuspended",
            "Member is not suspended.");

    public static readonly Error MemberNotFound =
        new(
            "Membership.MemberNotFound",
            "Member not found in organization.");

    public static readonly Error CannotTransferOwnershipToSelf =
        new(
            "Membership.CannotTransferOwnershipToSelf",
            "Cannot transfer ownership to yourself.");

    public static readonly Error TargetMemberNotFound =
        new(
            "Membership.TargetMemberNotFound",
            "Target member not found in organization.");

    public static readonly Error TargetMemberNotActive =
        new(
            "Membership.TargetMemberNotActive",
            "Target member must be active to receive ownership.");

    public static readonly Error TargetMemberAlreadyOwner =
        new(
            "Membership.TargetMemberAlreadyOwner",
            "Target member is already an owner.");

    public static readonly Error OnlyOwnerCanTransferOwnership =
        new(
            "Membership.OnlyOwnerCanTransferOwnership",
            "Only the organization owner can transfer ownership.");
}