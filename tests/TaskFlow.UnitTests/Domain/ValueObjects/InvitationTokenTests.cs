using FluentAssertions;
using TaskFlow.Domain.ValueObjects;
using TaskFlow.Domain.Common;

namespace TaskFlow.UnitTests.Domain.ValueObjects;

public class InvitationTokenTests
{
    [Fact]
    public void Create_WithValidToken_ReturnsSuccess()
    {
        // Arrange
        var validToken = new string('a', 32);

        // Act
        var result = InvitationToken.Create(validToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(validToken);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithNullOrEmpty_ReturnsFailure(string? invalidToken)
    {
        // Act
        var result = InvitationToken.Create(invalidToken!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("invitation.token_empty");
    }

    [Fact]
    public void Create_WithTooShort_ReturnsFailure()
    {
        // Arrange
        var shortToken = new string('a', 31);

        // Act
        var result = InvitationToken.Create(shortToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("invitation.token_invalid");
    }

    [Fact]
    public void Generate_ReturnsValidToken()
    {
        // Act
        var token = InvitationToken.Generate();

        // Assert
        token.Value.Should().NotBeNullOrEmpty();
        token.Value.Length.Should().BeGreaterThanOrEqualTo(32);
    }

    [Fact]
    public void Equality_SameValue_ReturnsTrue()
    {
        // Arrange
        var token1 = InvitationToken.Create(new string('a', 32)).Value;
        var token2 = InvitationToken.Create(new string('a', 32)).Value;

        // Act & Assert
        token1.Should().Be(token2);
        (token1 == token2).Should().BeTrue();
        token1.GetHashCode().Should().Be(token2.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValue_ReturnsFalse()
    {
        // Arrange
        var token1 = InvitationToken.Create(new string('a', 32)).Value;
        var token2 = InvitationToken.Create(new string('b', 32)).Value;

        // Act & Assert
        token1.Should().NotBe(token2);
        (token1 == token2).Should().BeFalse();
    }

    [Fact]
    public void ImplicitConversion_ToString_Works()
    {
        // Arrange
        var token = InvitationToken.Create(new string('a', 32)).Value;

        // Act
        string tokenString = token.Value;

        // Assert
        tokenString.Should().Be(new string('a', 32));
    }

    [Fact]
    public void ToString_ReturnsTokenValue()
    {
        // Arrange
        var token = InvitationToken.Create(new string('a', 32)).Value;

        // Act
        var result = token.ToString();

        // Assert
        result.Should().Be(new string('a', 32));
    }

    [Fact]
    public void Generate_ProducesUniqueTokens()
    {
        // Act
        var token1 = InvitationToken.Generate();
        var token2 = InvitationToken.Generate();

        // Assert
        token1.Value.Should().NotBe(token2.Value);
    }
}