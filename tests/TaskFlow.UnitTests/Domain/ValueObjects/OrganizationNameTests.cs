using FluentAssertions;
using TaskFlow.Domain.ValueObjects;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.Constants;

namespace TaskFlow.UnitTests.Domain.ValueObjects;

public class OrganizationNameTests
{
    [Fact]
    public void Create_WithValidName_ReturnsSuccess()
    {
        // Arrange
        const string validName = "Acme Corp";

        // Act
        var result = OrganizationName.Create(validName);

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
        var result = OrganizationName.Create(invalidName!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrganizationErrors.NameRequired);
    }

    [Fact]
    public void Create_WithTooShort_ReturnsFailure()
    {
        // Arrange
        var shortName = new string('a', OrganizationConstants.NameMinLength - 1);

        // Act
        var result = OrganizationName.Create(shortName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrganizationErrors.NameTooShort);
    }

    [Fact]
    public void Create_WithTooLong_ReturnsFailure()
    {
        // Arrange
        var longName = new string('a', OrganizationConstants.NameMaxLength + 1);

        // Act
        var result = OrganizationName.Create(longName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrganizationErrors.NameTooLong);
    }

    [Fact]
    public void Create_WithMinLength_ReturnsSuccess()
    {
        // Arrange
        var minName = new string('a', OrganizationConstants.NameMinLength);

        // Act
        var result = OrganizationName.Create(minName);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithMaxLength_ReturnsSuccess()
    {
        // Arrange
        var maxName = new string('a', OrganizationConstants.NameMaxLength);

        // Act
        var result = OrganizationName.Create(maxName);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithWhitespace_TrimsAndValidates()
    {
        // Act
        var result = OrganizationName.Create("  Acme Corp  ");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("Acme Corp");
    }

    [Fact]
    public void Equality_SameValue_ReturnsTrue()
    {
        // Arrange
        var name1 = OrganizationName.Create("Acme Corp").Value;
        var name2 = OrganizationName.Create("Acme Corp").Value;

        // Act & Assert
        name1.Should().Be(name2);
        (name1 == name2).Should().BeTrue();
        name1.GetHashCode().Should().Be(name2.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValue_ReturnsFalse()
    {
        // Arrange
        var name1 = OrganizationName.Create("Acme Corp").Value;
        var name2 = OrganizationName.Create("Beta Inc").Value;

        // Act & Assert
        name1.Should().NotBe(name2);
        (name1 == name2).Should().BeFalse();
    }

    [Fact]
    public void ToString_ReturnsOrganizationNameValue()
    {
        // Arrange
        var name = OrganizationName.Create("Acme Corp").Value;

        // Act
        var result = name.ToString();

        // Assert
        result.Should().Be("Acme Corp");
    }
}