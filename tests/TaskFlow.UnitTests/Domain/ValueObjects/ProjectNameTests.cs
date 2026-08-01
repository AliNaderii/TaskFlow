using FluentAssertions;
using TaskFlow.Domain.ValueObjects;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.Constants;

namespace TaskFlow.UnitTests.Domain.ValueObjects;

public class ProjectNameTests
{
    [Fact]
    public void Create_WithValidProjectName_ReturnsSuccess()
    {
        // Arrange
        const string validName = "Project Alpha";

        // Act
        var result = ProjectName.Create(validName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(validName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithNullOrEmpty_ReturnsFailure(string? invalidName)
    {
        // Act
        var result = ProjectName.Create(invalidName!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProjectErrors.NameRequired);
    }

    [Fact]
    public void Create_WithTooShort_ReturnsFailure()
    {
        // Arrange
        var shortName = new string('a', ProjectConstants.NameMinLength - 1);

        // Act
        var result = ProjectName.Create(shortName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProjectErrors.NameTooShort);
    }

    [Fact]
    public void Create_WithTooLong_ReturnsFailure()
    {
        // Arrange
        var longName = new string('a', ProjectConstants.NameMaxLength + 1);

        // Act
        var result = ProjectName.Create(longName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ProjectErrors.NameTooLong);
    }

    [Fact]
    public void Create_WithMinLength_ReturnsSuccess()
    {
        // Arrange
        var minName = new string('a', ProjectConstants.NameMinLength);

        // Act
        var result = ProjectName.Create(minName);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithMaxLength_ReturnsSuccess()
    {
        // Arrange
        var maxName = new string('a', ProjectConstants.NameMaxLength);

        // Act
        var result = ProjectName.Create(maxName);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithWhitespace_TrimsAndValidates()
    {
        // Act
        var result = ProjectName.Create("  Project Alpha  ");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("Project Alpha");
    }

    [Fact]
    public void Equality_SameValue_ReturnsTrue()
    {
        // Arrange
        var name1 = ProjectName.Create("Project Alpha").Value;
        var name2 = ProjectName.Create("Project Alpha").Value;

        // Act & Assert
        name1.Should().Be(name2);
        (name1 == name2).Should().BeTrue();
        name1.GetHashCode().Should().Be(name2.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValue_ReturnsFalse()
    {
        // Arrange
        var name1 = ProjectName.Create("Project Alpha").Value;
        var name2 = ProjectName.Create("Project Beta").Value;

        // Act & Assert
        name1.Should().NotBe(name2);
        (name1 == name2).Should().BeFalse();
    }

    [Fact]
    public void ToString_ReturnsProjectNameValue()
    {
        // Arrange
        var name = ProjectName.Create("Project Alpha").Value;

        // Act
        var result = name.ToString();

        // Assert
        result.Should().Be("Project Alpha");
    }
}