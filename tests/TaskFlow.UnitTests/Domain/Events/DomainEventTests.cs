using FluentAssertions;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Events;

namespace TaskFlow.UnitTests.Domain.Events;

public class DomainEventTests
{
    [Fact]
    public void TaskAssignedEvent_Create_ReturnsEventWithCorrectProperties()
    {
        // Arrange
        var taskItemId = Guid.NewGuid();
        var assigneeUserId = Guid.NewGuid();
        var assignedByUserId = Guid.NewGuid();

        // Act
        var domainEvent = TaskAssignedEvent.Create(taskItemId, assigneeUserId, assignedByUserId);

        // Assert
        domainEvent.TaskItemId.Should().Be(taskItemId);
        domainEvent.AssigneeUserId.Should().Be(assigneeUserId);
        domainEvent.AssignedByUserId.Should().Be(assignedByUserId);
        domainEvent.EventId.Should().NotBeEmpty();
        domainEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void TaskAssignedEvent_ImplementsIDomainEvent()
    {
        // Act
        var domainEvent = TaskAssignedEvent.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Assert
        domainEvent.Should().BeAssignableTo<IDomainEvent>();
    }

    [Fact]
    public void CommentCreatedEvent_Create_ReturnsEventWithCorrectProperties()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var taskItemId = Guid.NewGuid();
        var authorUserId = Guid.NewGuid();

        // Act
        var domainEvent = CommentCreatedEvent.Create(commentId, taskItemId, authorUserId);

        // Assert
        domainEvent.CommentId.Should().Be(commentId);
        domainEvent.TaskItemId.Should().Be(taskItemId);
        domainEvent.AuthorUserId.Should().Be(authorUserId);
        domainEvent.EventId.Should().NotBeEmpty();
        domainEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CommentCreatedEvent_ImplementsIDomainEvent()
    {
        // Act
        var domainEvent = CommentCreatedEvent.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Assert
        domainEvent.Should().BeAssignableTo<IDomainEvent>();
    }

    [Fact]
    public void ProjectArchivedEvent_Create_ReturnsEventWithCorrectProperties()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var archivedByUserId = Guid.NewGuid();

        // Act
        var domainEvent = ProjectArchivedEvent.Create(projectId, archivedByUserId);

        // Assert
        domainEvent.ProjectId.Should().Be(projectId);
        domainEvent.ArchivedByUserId.Should().Be(archivedByUserId);
        domainEvent.EventId.Should().NotBeEmpty();
        domainEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ProjectArchivedEvent_ImplementsIDomainEvent()
    {
        // Act
        var domainEvent = ProjectArchivedEvent.Create(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        domainEvent.Should().BeAssignableTo<IDomainEvent>();
    }

    [Fact]
    public void MembershipAddedEvent_Create_ReturnsEventWithCorrectProperties()
    {
        // Arrange
        var membershipId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        const string role = "Member";

        // Act
        var domainEvent = MembershipAddedEvent.Create(membershipId, organizationId, userId, role);

        // Assert
        domainEvent.MembershipId.Should().Be(membershipId);
        domainEvent.OrganizationId.Should().Be(organizationId);
        domainEvent.UserId.Should().Be(userId);
        domainEvent.Role.Should().Be(role);
        domainEvent.EventId.Should().NotBeEmpty();
        domainEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MembershipAddedEvent_ImplementsIDomainEvent()
    {
        // Act
        var domainEvent = MembershipAddedEvent.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Admin");

        // Assert
        domainEvent.Should().BeAssignableTo<IDomainEvent>();
    }
}