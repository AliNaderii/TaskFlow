using FluentAssertions;
using TaskFlow.Domain.ValueObjects;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.Constants;

namespace TaskFlow.UnitTests.Domain.ValueObjects;

public class DisplayNameTests
{
    [Fact]
    public void Create_WithValidDisplayName_ReturnsSuccess()
    {
        // Arrange
        const string validName = "John Doe";

        // Act
        var result = DisplayName.Create(validName);

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
        var result = DisplayName.Create(invalidName!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DisplayNameErrors.Required);
    }

    [Fact]
    public void Create_WithTooShort_ReturnsFailure()
    {
        // Arrange
        var shortName = new string('a', DisplayNameConstants.MinLength - 1);

        // Act
        var result = DisplayName.Create(shortName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DisplayNameErrors.TooShort);
    }

    [Fact]
    public void Create_WithTooLong_ReturnsFailure()
    {
        // Arrange
        var longName = new string('a', DisplayNameConstants.MaxLength + 1);

        // Act
        var result = DisplayName.Create(longName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DisplayNameErrors.TooLong);
    }

    [Fact]
    public void Create_WithMinLength_ReturnsSuccess()
    {
        // Arrange
        var minName = new string('a', DisplayNameConstants.MinLength);

        // Act
        var result = DisplayName.Create(minName);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithMaxLength_ReturnsSuccess()
    {
        // Arrange
        var maxName = new string('a', DisplayNameConstants.MaxLength);

        // Act
        var result = DisplayName.Create(maxName);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithWhitespace_TrimsAndValidates()
    {
        // Act
        var result = DisplayName.Create("  John Doe  ");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("John Doe");
    }

    [Fact]
    public void Equality_SameValue_ReturnsTrue()
    {
        // Arrange
        var name1 = DisplayName.Create("John Doe").Value;
        var name2 = DisplayName.Create("John Doe").Value;

        // Act & Assert
        name1.Should().Be(name2);
        (name1 == name2).Should().BeTrue();
        name1.GetHashCode().Should().Be(name2.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValue_ReturnsFalse()
    {
        // Arrange
        var name1 = DisplayName.Create("John Doe").Value;
        var name2 = DisplayName.Create("Jane Doe").Value;

        // Act & Assert
        name1.Should().NotBe(name2);
        (name1 == name2).Should().BeFalse();
    }

    [Fact]
    public void ToString_ReturnsRecordToString()
    {
        // Arrange
        var name = DisplayName.Create("John Doe").Value;

        // Act
        var result = name.ToString();

        // Assert
        result.Should().Be("DisplayName { Value = John Doe }");
    }
}