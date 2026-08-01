using FluentAssertions;
using TaskFlow.Domain.ValueObjects;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.Constants;

namespace TaskFlow.UnitTests.Domain.ValueObjects;

public class ProjectDescriptionTests
{
    [Fact]
    public void Create_WithValidDescription_ReturnsSuccess()
    {
        // Arrange
        const string validDescription = "This is a valid project description.";

        // Act
        var result = ProjectDescription.Create(validDescription);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Value.Should().Be(validDescription);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithNullOrEmpty_ReturnsSuccessWithNull(string? emptyDescription)
    {
        // Act
        var result = ProjectDescription.Create(emptyDescription!);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public void Create_WithTooLong_ReturnsFailure()
    {
        // Arrange
        var longDescription = new string('a', ProjectConstants.DescriptionMaxLength + 1);

        // Act
        var result = ProjectDescription.Create(longDescription);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProjectErrors.DescriptionTooLong);
    }

    [Fact]
    public void Create_WithMaxLength_ReturnsSuccess()
    {
        // Arrange
        var maxDescription = new string('a', ProjectConstants.DescriptionMaxLength);

        // Act
        var result = ProjectDescription.Create(maxDescription);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithWhitespace_TrimsAndValidates()
    {
        // Act
        var result = ProjectDescription.Create("  Valid description  ");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Value.Should().Be("Valid description");
    }

    [Fact]
    public void Equality_SameValue_ReturnsTrue()
    {
        // Arrange
        var desc1 = ProjectDescription.Create("Same description").Value!;
        var desc2 = ProjectDescription.Create("Same description").Value!;

        // Act & Assert
        desc1.Should().Be(desc2);
        (desc1 == desc2).Should().BeTrue();
        desc1.GetHashCode().Should().Be(desc2.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValue_ReturnsFalse()
    {
        // Arrange
        var desc1 = ProjectDescription.Create("First description").Value!;
        var desc2 = ProjectDescription.Create("Second description").Value!;

        // Act & Assert
        desc1.Should().NotBe(desc2);
        (desc1 == desc2).Should().BeFalse();
    }

    [Fact]
    public void Equality_NullValues_ReturnsTrue()
    {
        // Arrange
        var desc1 = ProjectDescription.Create("").Value;
        var desc2 = ProjectDescription.Create(null).Value;

        // Act & Assert
        desc1.Should().BeNull();
        desc2.Should().BeNull();
    }

    [Fact]
    public void ToString_ReturnsDescriptionValue()
    {
        // Arrange
        var desc = ProjectDescription.Create("Test description").Value!;

        // Act
        var result = desc.ToString();

        // Assert
        result.Should().Be("Test description");
    }
}