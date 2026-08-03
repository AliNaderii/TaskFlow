using FluentAssertions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;
using TaskFlow.Domain.Errors;

namespace TaskFlow.UnitTests.Domain.Entities;

public class OrganizationTests
{
    [Fact]
    public void Create_WithValidName_ReturnsSuccess()
    {
        // Arrange
        var name = OrganizationName.Create("Test Organization").Value;

        // Act
        var result = Organization.Create(name);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(name);
        result.Value.ArchivedAt.Should().BeNull();
    }

    [Fact]
    public void UpdateName_WithValidName_UpdatesName()
    {
        // Arrange
        var name = OrganizationName.Create("Test Organization").Value;
        var organization = Organization.Create(name).Value;

        var newName = OrganizationName.Create("New Organization Name").Value;

        // Act
        var result = organization.UpdateName(newName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        organization.Name.Should().Be(newName);
    }

    [Fact]
    public void UpdateName_WithSameName_ReturnsSuccessWithoutChange()
    {
        // Arrange
        var name = OrganizationName.Create("Test Organization").Value;
        var organization = Organization.Create(name).Value;

        // Act
        var result = organization.UpdateName(name);

        // Assert
        result.IsSuccess.Should().BeTrue();
        organization.Name.Should().Be(name);
    }

    [Fact]
    public void Archive_WhenNotArchived_ArchivesOrganization()
    {
        // Arrange
        var name = OrganizationName.Create("Test Organization").Value;
        var organization = Organization.Create(name).Value;

        // Act
        var result = organization.Archive();

        // Assert
        result.IsSuccess.Should().BeTrue();
        organization.ArchivedAt.Should().NotBeNull();
    }

    [Fact]
    public void Archive_WhenAlreadyArchived_ReturnsFailure()
    {
        // Arrange
        var name = OrganizationName.Create("Test Organization").Value;
        var organization = Organization.Create(name).Value;
        organization.Archive();

        // Act
        var result = organization.Archive();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrganizationErrors.AlreadyArchived);
    }
}