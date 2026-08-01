using FluentAssertions;
using TaskFlow.Domain.ValueObjects;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.Constants;

namespace TaskFlow.UnitTests.Domain.ValueObjects;

public class CommentContentTests
{
    [Fact]
    public void Create_WithValidContent_ReturnsSuccess()
    {
        // Arrange
        const string validContent = "This is a valid comment.";

        // Act
        var result = CommentContent.Create(validContent);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(validContent);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithNullOrEmpty_ReturnsFailure(string? invalidContent)
    {
        // Act
        var result = CommentContent.Create(invalidContent!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CommentErrors.ContentRequired);
    }

    [Fact]
    public void Create_WithTooShort_ReturnsFailure()
    {
        // Arrange
        var shortContent = new string('a', CommentConstants.ContentMinLength - 1);

        // Act
        var result = CommentContent.Create(shortContent);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CommentErrors.ContentTooShort);
    }

    [Fact]
    public void Create_WithTooLong_ReturnsFailure()
    {
        // Arrange
        var longContent = new string('a', CommentConstants.ContentMaxLength + 1);

        // Act
        var result = CommentContent.Create(longContent);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CommentErrors.ContentTooLong);
    }

    [Fact]
    public void Create_WithMinLength_ReturnsSuccess()
    {
        // Arrange
        var minContent = new string('a', CommentConstants.ContentMinLength);

        // Act
        var result = CommentContent.Create(minContent);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithMaxLength_ReturnsSuccess()
    {
        // Arrange
        var maxContent = new string('a', CommentConstants.ContentMaxLength);

        // Act
        var result = CommentContent.Create(maxContent);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithWhitespace_TrimsAndValidates()
    {
        // Act
        var result = CommentContent.Create("  This is a valid comment.  ");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("This is a valid comment.");
    }

    [Fact]
    public void Equality_SameValue_ReturnsTrue()
    {
        // Arrange
        var content1 = CommentContent.Create("This is a comment").Value;
        var content2 = CommentContent.Create("This is a comment").Value;

        // Act & Assert
        content1.Should().Be(content2);
        (content1 == content2).Should().BeTrue();
        content1.GetHashCode().Should().Be(content2.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValue_ReturnsFalse()
    {
        // Arrange
        var content1 = CommentContent.Create("First comment").Value;
        var content2 = CommentContent.Create("Second comment").Value;

        // Act & Assert
        content1.Should().NotBe(content2);
        (content1 == content2).Should().BeFalse();
    }

    [Fact]
    public void ToString_ReturnsContentValue()
    {
        // Arrange
        var content = CommentContent.Create("This is a comment").Value;

        // Act
        var result = content.ToString();

        // Assert
        result.Should().Be("This is a comment");
    }
}