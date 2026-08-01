using FluentAssertions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.Events;

namespace TaskFlow.UnitTests.Domain.Entities;

public class TaskItemTests
{
    private readonly Guid _organizationId = Guid.NewGuid();
    private readonly Guid _projectId = Guid.NewGuid();
    private readonly Guid _creatorUserId = Guid.NewGuid();
    private readonly TaskItemTitle _validTitle = TaskItemTitle.Create("Test Task").Value!;
    private readonly TaskItemDescription _validDescription = TaskItemDescription.Create("Test description").Value!;
    private readonly TaskItemPriority _priority = TaskItemPriority.Medium;

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        // Act
        var result = TaskItem.Create(
            _organizationId,
            _projectId,
            _creatorUserId,
            _validTitle,
            _validDescription,
            _priority,
            null,
            null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.OrganizationId.Should().Be(_organizationId);
        result.Value.ProjectId.Should().Be(_projectId);
        result.Value.CreatorUserId.Should().Be(_creatorUserId);
        result.Value.Title.Should().Be(_validTitle);
        result.Value.Description.Should().Be(_validDescription);
        result.Value.Priority.Should().Be(_priority);
        result.Value.Status.Should().Be(TaskItemStatus.Todo);
        result.Value.IsArchived.Should().BeFalse();
    }

    [Fact]
    public void Create_WithEmptyCreatorUserId_ReturnsFailure()
    {
        // Act
        var result = TaskItem.Create(
            _organizationId,
            _projectId,
            Guid.Empty,
            _validTitle,
            _validDescription,
            _priority,
            null,
            null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TaskItemErrors.InvalidCreatorUserId);
    }

    [Fact]
    public void Create_WithEmptyOrganizationId_ReturnsFailure()
    {
        // Act
        var result = TaskItem.Create(
            Guid.Empty,
            _projectId,
            _creatorUserId,
            _validTitle,
            _validDescription,
            _priority,
            null,
            null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TaskItemErrors.InvalidOrganizationId);
    }

    [Fact]
    public void Create_WithAssignee_SetsAssigneeUserId()
    {
        // Arrange
        var assigneeUserId = Guid.NewGuid();

        // Act
        var result = TaskItem.Create(
            _organizationId,
            _projectId,
            _creatorUserId,
            _validTitle,
            _validDescription,
            _priority,
            null,
            assigneeUserId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AssigneeUserId.Should().Be(assigneeUserId);
    }

    [Fact]
    public void Create_WithDueDate_SetsDueDate()
    {
        // Arrange
        var dueDate = DateTime.UtcNow.AddDays(7);

        // Act
        var result = TaskItem.Create(
            _organizationId,
            _projectId,
            _creatorUserId,
            _validTitle,
            _validDescription,
            _priority,
            dueDate,
            null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.DueDate.Should().Be(dueDate);
    }

    [Fact]
    public void Rename_ValidTitle_UpdatesTitle()
    {
        // Arrange
        var taskItem = TaskItem.Create(
            _organizationId,
            _projectId,
            _creatorUserId,
            _validTitle,
            _validDescription,
            _priority,
            null,
            null).Value;
        var newTitle = "Updated Task Title";

        // Act
        var result = taskItem.Rename(newTitle);

        // Assert
        result.IsSuccess.Should().BeTrue();
        taskItem.Title.Value.Should().Be(newTitle);
    }

    [Fact]
    public void Rename_InvalidTitle_ReturnsFailure()
    {
        // Arrange
        var taskItem = TaskItem.Create(
            _organizationId,
            _projectId,
            _creatorUserId,
            _validTitle,
            _validDescription,
            _priority,
            null,
            null).Value;

        // Act
        var result = taskItem.Rename(""); // Empty title is invalid

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TaskItemErrors.TitleRequired);
    }

    [Fact]
    public void ChangeDescription_ValidDescription_UpdatesDescription()
    {
        // Arrange
        var taskItem = TaskItem.Create(
            _organizationId,
            _projectId,
            _creatorUserId,
            _validTitle,
            _validDescription,
            _priority,
            null,
            null).Value;
        var newDescription = "Updated description";

        // Act
        var result = taskItem.ChangeDescription(newDescription);

        // Assert
        result.IsSuccess.Should().BeTrue();
        taskItem.Description!.Value.Should().Be(newDescription);
    }

    [Fact]
    public void ChangeDescription_NullDescription_SetsToNull()
    {
        // Arrange
        var taskItem = TaskItem.Create(
            _organizationId,
            _projectId,
            _creatorUserId,
            _validTitle,
            _validDescription,
            _priority,
            null,
            null).Value;

        // Act
        var result = taskItem.ChangeDescription(null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        taskItem.Description.Should().BeNull();
    }

    [Fact]
    public void ChangePriority_UpdatesPriority()
    {
        // Arrange
        var taskItem = TaskItem.Create(
            _organizationId,
            _projectId,
            _creatorUserId,
            _validTitle,
            _validDescription,
            _priority,
            null,
            null).Value;

        // Act
        var result = taskItem.ChangePriority(TaskItemPriority.High);

        // Assert
        result.IsSuccess.Should().BeTrue();
        taskItem.Priority.Should().Be(TaskItemPriority.High);
    }

    [Fact]
    public void ChangeDueDate_UpdatesDueDate()
    {
        // Arrange
        var taskItem = TaskItem.Create(
            _organizationId,
            _projectId,
            _creatorUserId,
            _validTitle,
            _validDescription,
            _priority,
            null,
            null).Value;
        var newDueDate = DateTime.UtcNow.AddDays(14);

        // Act
        var result = taskItem.ChangeDueDate(newDueDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        taskItem.DueDate.Should().Be(newDueDate);
    }

    [Fact]
    public void AssignTo_ValidUser_SucceedsAndAddsEvent()
    {
        // Arrange
        var taskItem = TaskItem.Create(
            _organizationId,
            _projectId,
            _creatorUserId,
            _validTitle,
            _validDescription,
            _priority,
            null,
            null).Value;
        var assigneeUserId = Guid.NewGuid();
        var assignedByUserId = Guid.NewGuid();

        // Act
        var result = taskItem.AssignTo(assigneeUserId, assignedByUserId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        taskItem.AssigneeUserId.Should().Be(assigneeUserId);
        taskItem.DomainEvents.Should().HaveCount(1);
        taskItem.DomainEvents.First().Should().BeOfType<TaskAssignedEvent>();
        var assignedEvent = (TaskAssignedEvent)taskItem.DomainEvents.First();
        assignedEvent.TaskItemId.Should().Be(taskItem.Id);
        assignedEvent.AssigneeUserId.Should().Be(assigneeUserId);
        assignedEvent.AssignedByUserId.Should().Be(assignedByUserId);
    }

    [Fact]
    public void AssignTo_AlreadyAssignedToSameUser_ReturnsFailure()
    {
        // Arrange
        var assigneeUserId = Guid.NewGuid();
        var taskItem = TaskItem.Create(
            _organizationId,
            _projectId,
            _creatorUserId,
            _validTitle,
            _validDescription,
            _priority,
            null,
            assigneeUserId).Value;
        var assignedByUserId = Guid.NewGuid();

        // Act
        var result = taskItem.AssignTo(assigneeUserId, assignedByUserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TaskItemErrors.AlreadyAssignedToUser);
    }

    [Fact]
    public void AssignTo_ArchivedTask_ReturnsFailure()
    {
        // Arrange
        var taskItem = TaskItem.Create(
            _organizationId,
            _projectId,
            _creatorUserId,
            _validTitle,
            _validDescription,
            _priority,
            null,
            null).Value;
        taskItem.Archive();
        var assigneeUserId = Guid.NewGuid();
        var assignedByUserId = Guid.NewGuid();

        // Act
        var result = taskItem.AssignTo(assigneeUserId, assignedByUserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TaskItemErrors.AlreadyArchived);
    }

    [Fact]
    public void Unassign_AssignedTask_Succeeds()
    {
        // Arrange
        var assigneeUserId = Guid.NewGuid();
        var taskItem = TaskItem.Create(
            _organizationId,
            _projectId,
            _creatorUserId,
            _validTitle,
            _validDescription,
            _priority,
            null,
            assigneeUserId).Value;

        // Act
        var result = taskItem.Unassign();

        // Assert
        result.IsSuccess.Should().BeTrue();
        taskItem.AssigneeUserId.Should().BeNull();
    }

    [Fact]
    public void Unassign_NotAssigned_ReturnsFailure()
    {
        // Arrange
        var taskItem = TaskItem.Create(
            _organizationId,
            _projectId,
            _creatorUserId,
            _validTitle,
            _validDescription,
            _priority,
            null,
            null).Value;

        // Act
        var result = taskItem.Unassign();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TaskItemErrors.NotAssigned);
    }

    [Fact]
    public void ChangeStatus_ValidChange_UpdatesStatus()
    {
        // Arrange
        var taskItem = TaskItem.Create(
            _organizationId,
            _projectId,
            _creatorUserId,
            _validTitle,
            _validDescription,
            _priority,
            null,
            null).Value;
        var changedByUserId = Guid.NewGuid();

        // Act
        var result = taskItem.ChangeStatus(TaskItemStatus.InProgress, changedByUserId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        taskItem.Status.Should().Be(TaskItemStatus.InProgress);
    }

    [Fact]
    public void ChangeStatus_SameStatus_ReturnsFailure()
    {
        // Arrange
        var taskItem = TaskItem.Create(
            _organizationId,
            _projectId,
            _creatorUserId,
            _validTitle,
            _validDescription,
            _priority,
            null,
            null).Value;
        var changedByUserId = Guid.NewGuid();

        // Act
        var result = taskItem.ChangeStatus(TaskItemStatus.Todo, changedByUserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TaskItemErrors.StatusAlreadySet);
    }

    [Fact]
    public void ChangeStatus_ToDone_AddsCompletedEvent()
    {
        // Arrange
        var taskItem = TaskItem.Create(
            _organizationId,
            _projectId,
            _creatorUserId,
            _validTitle,
            _validDescription,
            _priority,
            null,
            null).Value;
        var changedByUserId = Guid.NewGuid();

        // Act
        var result = taskItem.ChangeStatus(TaskItemStatus.Done, changedByUserId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        taskItem.Status.Should().Be(TaskItemStatus.Done);
        taskItem.DomainEvents.Should().HaveCount(1);
        taskItem.DomainEvents.First().Should().BeOfType<TaskCompletedEvent>();
        var completedEvent = (TaskCompletedEvent)taskItem.DomainEvents.First();
        completedEvent.TaskItemId.Should().Be(taskItem.Id);
        completedEvent.CompletedByUserId.Should().Be(changedByUserId);
    }

    [Fact]
    public void ChangeStatus_ArchivedTask_ReturnsFailure()
    {
        // Arrange
        var taskItem = TaskItem.Create(
            _organizationId,
            _projectId,
            _creatorUserId,
            _validTitle,
            _validDescription,
            _priority,
            null,
            null).Value;
        taskItem.Archive();
        var changedByUserId = Guid.NewGuid();

        // Act
        var result = taskItem.ChangeStatus(TaskItemStatus.InProgress, changedByUserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TaskItemErrors.AlreadyArchived);
    }

    [Fact]
    public void Archive_ActiveTask_Succeeds()
    {
        // Arrange
        var taskItem = TaskItem.Create(
            _organizationId,
            _projectId,
            _creatorUserId,
            _validTitle,
            _validDescription,
            _priority,
            null,
            null).Value;

        // Act
        var result = taskItem.Archive();

        // Assert
        result.IsSuccess.Should().BeTrue();
        taskItem.IsArchived.Should().BeTrue();
    }

    [Fact]
    public void Archive_AlreadyArchived_ReturnsFailure()
    {
        // Arrange
        var taskItem = TaskItem.Create(
            _organizationId,
            _projectId,
            _creatorUserId,
            _validTitle,
            _validDescription,
            _priority,
            null,
            null).Value;
        taskItem.Archive();

        // Act
        var result = taskItem.Archive();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TaskItemErrors.AlreadyArchived);
    }

    [Fact]
    public void Restore_ArchivedTask_Succeeds()
    {
        // Arrange
        var taskItem = TaskItem.Create(
            _organizationId,
            _projectId,
            _creatorUserId,
            _validTitle,
            _validDescription,
            _priority,
            null,
            null).Value;
        taskItem.Archive();

        // Act
        var result = taskItem.Restore();

        // Assert
        result.IsSuccess.Should().BeTrue();
        taskItem.IsArchived.Should().BeFalse();
    }

    [Fact]
    public void Restore_ActiveTask_ReturnsFailure()
    {
        // Arrange
        var taskItem = TaskItem.Create(
            _organizationId,
            _projectId,
            _creatorUserId,
            _validTitle,
            _validDescription,
            _priority,
            null,
            null).Value;

        // Act
        var result = taskItem.Restore();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TaskItemErrors.NotArchived);
    }
}