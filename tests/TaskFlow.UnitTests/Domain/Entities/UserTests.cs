using FluentAssertions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;
using TaskFlow.Domain.Errors;

namespace TaskFlow.UnitTests.Domain.Entities;

public class UserTests
{
    [Fact]
    public void Create_WithValidEmailAndDisplayName_ReturnsSuccess()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var displayName = DisplayName.Create("Test User").Value;

        // Act
        var result = User.Create(email, displayName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Email.Should().Be(email);
        result.Value.DisplayName.Should().Be(displayName);
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ChangeDisplayName_WithDifferentName_UpdatesDisplayName()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var displayName = DisplayName.Create("Test User").Value;
        var user = User.Create(email, displayName).Value;

        var newDisplayName = DisplayName.Create("New Name").Value;

        // Act
        var result = user.ChangeDisplayName(newDisplayName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.DisplayName.Should().Be(newDisplayName);
    }

    [Fact]
    public void ChangeDisplayName_WithSameName_ReturnsSuccessWithoutChange()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var displayName = DisplayName.Create("Test User").Value;
        var user = User.Create(email, displayName).Value;

        // Act
        var result = user.ChangeDisplayName(displayName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.DisplayName.Should().Be(displayName);
    }

    [Fact]
    public void DeActivate_WhenActive_DeactivatesUser()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var displayName = DisplayName.Create("Test User").Value;
        var user = User.Create(email, displayName).Value;

        // Act
        var result = user.DeActivate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void DeActivate_WhenAlreadyInactive_ReturnsSuccess()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var displayName = DisplayName.Create("Test User").Value;
        var user = User.Create(email, displayName).Value;
        user.DeActivate();

        // Act
        var result = user.DeActivate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_WhenInactive_ActivatesUser()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var displayName = DisplayName.Create("Test User").Value;
        var user = User.Create(email, displayName).Value;
        user.DeActivate();

        // Act
        var result = user.Activate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ReturnsSuccess()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var displayName = DisplayName.Create("Test User").Value;
        var user = User.Create(email, displayName).Value;

        // Act
        var result = user.Activate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.IsActive.Should().BeTrue();
    }
}