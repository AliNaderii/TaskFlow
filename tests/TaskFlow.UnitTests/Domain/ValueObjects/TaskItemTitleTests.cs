using FluentAssertions;
using TaskFlow.Domain.ValueObjects;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.Constants;

namespace TaskFlow.UnitTests.Domain.ValueObjects;

public class TaskItemTitleTests
{
    [Fact]
    public void Create_WithValidTitle_ReturnsSuccess()
    {
        // Arrange
        const string validTitle = "Complete the task";

        // Act
        var result = TaskItemTitle.Create(validTitle);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(validTitle);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithNullOrEmpty_ReturnsFailure(string? invalidTitle)
    {
        // Act
        var result = TaskItemTitle.Create(invalidTitle!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TaskItemErrors.TitleRequired);
    }

    [Fact]
    public void Create_WithTooShort_ReturnsFailure()
    {
        // Arrange
        var shortTitle = new string('a', TaskItemConstants.TitleMinLength - 1);

        // Act
        var result = TaskItemTitle.Create(shortTitle);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TaskItemErrors.TitleTooShort);
    }

    [Fact]
    public void Create_WithTooLong_ReturnsFailure()
    {
        // Arrange
        var longTitle = new string('a', TaskItemConstants.TitleMaxLength + 1);

        // Act
        var result = TaskItemTitle.Create(longTitle);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TaskItemErrors.TitleTooLong);
    }

    [Fact]
    public void Create_WithMinLength_ReturnsSuccess()
    {
        // Arrange
        var minTitle = new string('a', TaskItemConstants.TitleMinLength);

        // Act
        var result = TaskItemTitle.Create(minTitle);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithMaxLength_ReturnsSuccess()
    {
        // Arrange
        var maxTitle = new string('a', TaskItemConstants.TitleMaxLength);

        // Act
        var result = TaskItemTitle.Create(maxTitle);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithWhitespace_TrimsAndValidates()
    {
        // Act
        var result = TaskItemTitle.Create("  Complete the task  ");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("Complete the task");
    }

    [Fact]
    public void Equality_SameValue_ReturnsTrue()
    {
        // Arrange
        var title1 = TaskItemTitle.Create("Complete the task").Value;
        var title2 = TaskItemTitle.Create("Complete the task").Value;

        // Act & Assert
        title1.Should().Be(title2);
        (title1 == title2).Should().BeTrue();
        title1.GetHashCode().Should().Be(title2.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentValue_ReturnsFalse()
    {
        // Arrange
        var title1 = TaskItemTitle.Create("Complete the task").Value;
        var title2 = TaskItemTitle.Create("Another task").Value;

        // Act & Assert
        title1.Should().NotBe(title2);
        (title1 == title2).Should().BeFalse();
    }
}