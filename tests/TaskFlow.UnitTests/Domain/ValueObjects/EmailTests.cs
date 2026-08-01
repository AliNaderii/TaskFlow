using FluentAssertions;
using TaskFlow.Domain.ValueObjects;
using TaskFlow.Domain.Errors;

namespace TaskFlow.UnitTests.Domain.ValueObjects;

public class EmailTests
{
    [Fact]
    public void Create_WithValidEmail_ReturnsSuccess()
    {
        // Arrange
        const string validEmail = "test@example.com";

        // Act
        var result = Email.Create(validEmail);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(validEmail);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithNullOrEmpty_ReturnsFailure(string? invalidEmail)
    {
        // Act
        var result = Email.Create(invalidEmail!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EmailErrors.Required);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("invalid@")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test.example.com")]
    public void Create_WithInvalidFormat_ReturnsFailure(string invalidEmail)
    {
        // Act
        var result = Email.Create(invalidEmail);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(EmailErrors.Invalid);
    }

    [Fact]
    public void Create_WithWhitespace_TrimsAndValidates()
    {
        // Act
        var result = Email.Create("  test@example.com  ");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Equality_SameValue_ReturnsTrue()
    {
        // Arrange
        var email1 = Email.Create("test@example.com").Value;
        var email2 = Email.Create("test@example.com").Value;

        // Act & Assert
        email1.Should().Be(email2);
        (email1 == email2).Should().BeTrue();
        email1.GetHashCode().Should().Be(email2.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValue_ReturnsFalse()
    {
        // Arrange
        var email1 = Email.Create("test1@example.com").Value;
        var email2 = Email.Create("test2@example.com").Value;

        // Act & Assert
        email1.Should().NotBe(email2);
        (email1 == email2).Should().BeFalse();
    }

    [Fact]
    public void ToString_ReturnsEmailValue()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;

        // Act
        var result = email.ToString();

        // Assert
        result.Should().Be("test@example.com");
    }

    [Fact]
    public void ImplicitConversion_ToString_Works()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;

        // Act
        string emailString = email.Value;

        // Assert
        emailString.Should().Be("test@example.com");
    }
}
