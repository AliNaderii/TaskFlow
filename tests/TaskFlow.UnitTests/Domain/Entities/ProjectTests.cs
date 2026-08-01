using FluentAssertions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.Events;

namespace TaskFlow.UnitTests.Domain.Entities;

public class ProjectTests
{
    private readonly Guid _organizationId = Guid.NewGuid();
    private readonly ProjectName _validName = ProjectName.Create("Test Project").Value!;
    private readonly ProjectDescription _validDescription = ProjectDescription.Create("Test description").Value!;

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        // Act
        var result = Project.Create(_organizationId, _validName, _validDescription);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.OrganizationId.Should().Be(_organizationId);
        result.Value.Name.Should().Be(_validName);
        result.Value.Description.Should().Be(_validDescription);
        result.Value.IsArchived.Should().BeFalse();
    }

    [Fact]
    public void Create_WithNullDescription_ReturnsSuccess()
    {
        // Act
        var result = Project.Create(_organizationId, _validName, null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Description.Should().BeNull();
    }

    [Fact]
    public void Rename_SameName_ReturnsSuccess()
    {
        // Arrange
        var project = Project.Create(_organizationId, _validName, _validDescription).Value;

        // Act
        var result = project.Rename(_validName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.Name.Should().Be(_validName);
    }

    [Fact]
    public void Rename_DifferentName_UpdatesName()
    {
        // Arrange
        var project = Project.Create(_organizationId, _validName, _validDescription).Value;
        var newName = ProjectName.Create("New Project Name").Value!;

        // Act
        var result = project.Rename(newName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.Name.Should().Be(newName);
    }

    [Fact]
    public void ChangeDescription_SameDescription_ReturnsSuccess()
    {
        // Arrange
        var project = Project.Create(_organizationId, _validName, _validDescription).Value;

        // Act
        var result = project.ChangeDescription(_validDescription);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.Description.Should().Be(_validDescription);
    }

    [Fact]
    public void ChangeDescription_DifferentDescription_UpdatesDescription()
    {
        // Arrange
        var project = Project.Create(_organizationId, _validName, _validDescription).Value;
        var newDescription = ProjectDescription.Create("New description").Value!;

        // Act
        var result = project.ChangeDescription(newDescription);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.Description.Should().Be(newDescription);
    }

    [Fact]
    public void ChangeDescription_ToNull_Succeeds()
    {
        // Arrange
        var project = Project.Create(_organizationId, _validName, _validDescription).Value;

        // Act
        var result = project.ChangeDescription(null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.Description.Should().BeNull();
    }

    [Fact]
    public void Archive_ActiveProject_SucceedsAndAddsEvent()
    {
        // Arrange
        var project = Project.Create(_organizationId, _validName, _validDescription).Value;
        var archivedByUserId = Guid.NewGuid();

        // Act
        var result = project.Archive(archivedByUserId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.IsArchived.Should().BeTrue();
        project.DomainEvents.Should().HaveCount(1);
        project.DomainEvents.First().Should().BeOfType<ProjectArchivedEvent>();
        var archivedEvent = (ProjectArchivedEvent)project.DomainEvents.First();
        archivedEvent.ProjectId.Should().Be(project.Id);
        archivedEvent.ArchivedByUserId.Should().Be(archivedByUserId);
    }

    [Fact]
    public void Archive_AlreadyArchived_ReturnsFailure()
    {
        // Arrange
        var project = Project.Create(_organizationId, _validName, _validDescription).Value;
        project.Archive(Guid.NewGuid());

        // Act
        var result = project.Archive(Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProjectErrors.AlreadyArchived);
    }

    [Fact]
    public void Restore_ArchivedProject_Succeeds()
    {
        // Arrange
        var project = Project.Create(_organizationId, _validName, _validDescription).Value;
        project.Archive(Guid.NewGuid());

        // Act
        var result = project.Restore();

        // Assert
        result.IsSuccess.Should().BeTrue();
        project.IsArchived.Should().BeFalse();
    }

    [Fact]
    public void Restore_ActiveProject_ReturnsFailure()
    {
        // Arrange
        var project = Project.Create(_organizationId, _validName, _validDescription).Value;

        // Act
        var result = project.Restore();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProjectErrors.AlreadyActive);
    }
}