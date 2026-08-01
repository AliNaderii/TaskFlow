using FluentAssertions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.Events;

namespace TaskFlow.UnitTests.Domain.Entities;

public class MembershipTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _organizationId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        // Act
        var result = Membership.Create(_userId, _organizationId, MembershipRole.Member);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(_userId);
        result.Value.OrganizationId.Should().Be(_organizationId);
        result.Value.Role.Should().Be(MembershipRole.Member);
        result.Value.Status.Should().Be(MembershipStatus.Active);
    }

    [Fact]
    public void Create_WithEmptyUserId_ReturnsFailure()
    {
        // Act
        var result = Membership.Create(Guid.Empty, _organizationId, MembershipRole.Member);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("membership.invalid_user");
    }

    [Fact]
    public void Create_WithEmptyOrganizationId_ReturnsFailure()
    {
        // Act
        var result = Membership.Create(_userId, Guid.Empty, MembershipRole.Member);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("membership.invalid_organization");
    }

    [Fact]
    public void Create_AddsDomainEvent()
    {
        // Act
        var result = Membership.Create(_userId, _organizationId, MembershipRole.Admin);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.DomainEvents.Should().HaveCount(1);
        result.Value.DomainEvents.First().Should().BeOfType<MembershipAddedEvent>();
        var membershipEvent = (MembershipAddedEvent)result.Value.DomainEvents.First();
        membershipEvent.UserId.Should().Be(_userId);
        membershipEvent.OrganizationId.Should().Be(_organizationId);
        membershipEvent.Role.Should().Be("Admin");
    }

    [Fact]
    public void Remove_Owner_ReturnsFailure()
    {
        // Arrange
        var membership = Membership.Create(_userId, _organizationId, MembershipRole.Owner).Value;

        // Act
        var result = membership.Remove();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(MembershipErrors.CannotRemoveOwner);
    }

    [Fact]
    public void Remove_Member_Succeeds()
    {
        // Arrange
        var membership = Membership.Create(_userId, _organizationId, MembershipRole.Member).Value;

        // Act
        var result = membership.Remove();

        // Assert
        result.IsSuccess.Should().BeTrue();
        membership.Status.Should().Be(MembershipStatus.Removed);
    }

    [Fact]
    public void Remove_AlreadyRemoved_ReturnsSuccess()
    {
        // Arrange
        var membership = Membership.Create(_userId, _organizationId, MembershipRole.Member).Value;
        membership.Remove();

        // Act
        var result = membership.Remove();

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Leave_Owner_ReturnsFailure()
    {
        // Arrange
        var membership = Membership.Create(_userId, _organizationId, MembershipRole.Owner).Value;

        // Act
        var result = membership.Leave();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(MembershipErrors.CannotLeaveAsOwner);
    }

    [Fact]
    public void Leave_Member_Succeeds()
    {
        // Arrange
        var membership = Membership.Create(_userId, _organizationId, MembershipRole.Member).Value;

        // Act
        var result = membership.Leave();

        // Assert
        result.IsSuccess.Should().BeTrue();
        membership.Status.Should().Be(MembershipStatus.Left);
    }

    [Fact]
    public void ChangeRole_Owner_ReturnsFailure()
    {
        // Arrange
        var membership = Membership.Create(_userId, _organizationId, MembershipRole.Owner).Value;

        // Act
        var result = membership.ChangeRole(MembershipRole.Admin);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(MembershipErrors.CannotChangeOwnerRole);
    }

    [Fact]
    public void ChangeRole_ToOwner_ReturnsFailure()
    {
        // Arrange
        var membership = Membership.Create(_userId, _organizationId, MembershipRole.Member).Value;

        // Act
        var result = membership.ChangeRole(MembershipRole.Owner);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(MembershipErrors.CannotDemoteOwner);
    }

    [Fact]
    public void ChangeRole_SameRole_ReturnsSuccess()
    {
        // Arrange
        var membership = Membership.Create(_userId, _organizationId, MembershipRole.Member).Value;

        // Act
        var result = membership.ChangeRole(MembershipRole.Member);

        // Assert
        result.IsSuccess.Should().BeTrue();
        membership.Role.Should().Be(MembershipRole.Member);
    }

    [Fact]
    public void ChangeRole_ValidChange_Succeeds()
    {
        // Arrange
        var membership = Membership.Create(_userId, _organizationId, MembershipRole.Member).Value;

        // Act
        var result = membership.ChangeRole(MembershipRole.Admin);

        // Assert
        result.IsSuccess.Should().BeTrue();
        membership.Role.Should().Be(MembershipRole.Admin);
    }

    [Fact]
    public void Suspend_Owner_ReturnsFailure()
    {
        // Arrange
        var membership = Membership.Create(_userId, _organizationId, MembershipRole.Owner).Value;

        // Act
        var result = membership.Suspend();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(MembershipErrors.CannotChangeOwnerRole);
    }

    [Fact]
    public void Suspend_ActiveMember_Succeeds()
    {
        // Arrange
        var membership = Membership.Create(_userId, _organizationId, MembershipRole.Member).Value;

        // Act
        var result = membership.Suspend();

        // Assert
        result.IsSuccess.Should().BeTrue();
        membership.Status.Should().Be(MembershipStatus.Suspended);
    }

    [Fact]
    public void Suspend_AlreadySuspended_ReturnsFailure()
    {
        // Arrange
        var membership = Membership.Create(_userId, _organizationId, MembershipRole.Member).Value;
        membership.Suspend();

        // Act
        var result = membership.Suspend();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(MembershipErrors.AlreadySuspended);
    }

    [Fact]
    public void Activate_NotSuspended_ReturnsFailure()
    {
        // Arrange
        var membership = Membership.Create(_userId, _organizationId, MembershipRole.Member).Value;

        // Act
        var result = membership.Activate();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(MembershipErrors.NotSuspended);
    }

    [Fact]
    public void Activate_SuspendedMember_Succeeds()
    {
        // Arrange
        var membership = Membership.Create(_userId, _organizationId, MembershipRole.Member).Value;
        membership.Suspend();

        // Act
        var result = membership.Activate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        membership.Status.Should().Be(MembershipStatus.Active);
    }

    [Fact]
    public void TransferOwnership_NotOwner_ReturnsFailure()
    {
        // Arrange
        var membership = Membership.Create(_userId, _organizationId, MembershipRole.Admin).Value;
        var targetMembership = Membership.Create(Guid.NewGuid(), _organizationId, MembershipRole.Member).Value;

        // Act
        var result = membership.TransferOwnership(targetMembership);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(MembershipErrors.OnlyOwnerCanTransferOwnership);
    }

    [Fact]
    public void TransferOwnership_NullTarget_ReturnsFailure()
    {
        // Arrange
        var membership = Membership.Create(_userId, _organizationId, MembershipRole.Owner).Value;

        // Act
        var result = membership.TransferOwnership(null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(MembershipErrors.TargetMemberNotFound);
    }

    [Fact]
    public void TransferOwnership_ToSelf_ReturnsFailure()
    {
        // Arrange
        var membership = Membership.Create(_userId, _organizationId, MembershipRole.Owner).Value;

        // Act
        var result = membership.TransferOwnership(membership);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(MembershipErrors.CannotTransferOwnershipToSelf);
    }

    [Fact]
    public void TransferOwnership_TargetAlreadyOwner_ReturnsFailure()
    {
        // Arrange
        var membership = Membership.Create(_userId, _organizationId, MembershipRole.Owner).Value;
        var targetMembership = Membership.Create(Guid.NewGuid(), _organizationId, MembershipRole.Owner).Value;

        // Act
        var result = membership.TransferOwnership(targetMembership);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(MembershipErrors.TargetMemberAlreadyOwner);
    }

    [Fact]
    public void TransferOwnership_TargetNotActive_ReturnsFailure()
    {
        // Arrange
        var membership = Membership.Create(_userId, _organizationId, MembershipRole.Owner).Value;
        var targetMembership = Membership.Create(Guid.NewGuid(), _organizationId, MembershipRole.Member).Value;
        targetMembership.Suspend();

        // Act
        var result = membership.TransferOwnership(targetMembership);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(MembershipErrors.TargetMemberNotActive);
    }

    [Fact]
    public void TransferOwnership_ValidTransfer_Succeeds()
    {
        // Arrange
        var membership = Membership.Create(_userId, _organizationId, MembershipRole.Owner).Value;
        var targetUserId = Guid.NewGuid();
        var targetMembership = Membership.Create(targetUserId, _organizationId, MembershipRole.Member).Value;

        // Act
        var result = membership.TransferOwnership(targetMembership);

        // Assert
        result.IsSuccess.Should().BeTrue();
        membership.Role.Should().Be(MembershipRole.Admin);
        targetMembership.Role.Should().Be(MembershipRole.Owner);
    }
}