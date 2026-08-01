using FluentAssertions;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Errors;

namespace TaskFlow.UnitTests.Domain.Common;

public class ResultTests
{
    [Fact]
    public void Success_WithValue_ReturnsSuccessResult()
    {
        // Act
        var result = Result<string>.Success("test value");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be("test value");
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_WithError_ReturnsFailureResult()
    {
        // Arrange
        var error = new Error("Test.Error", "Test error message");

        // Act
        var result = Result<string>.Failure(error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void AccessingValue_OnFailure_ThrowsException()
    {
        // Arrange
        var error = new Error("Test.Error", "Test error message");
        var result = Result<string>.Failure(error);

        // Act & Assert
        var action = () => _ = result.Value;
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot access Value when result is failure.");
    }

    [Fact]
    public void BaseResult_Success_ReturnsSuccess()
    {
        // Act
        var result = BaseResult.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void BaseResult_Failure_ReturnsFailure()
    {
        // Arrange
        var error = new Error("Test.Error", "Test error message");

        // Act
        var result = BaseResult.Failure(error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Error_Equality_SameCodeAndMessage_ReturnsTrue()
    {
        // Arrange
        var error1 = new Error("Test.Error", "Test message");
        var error2 = new Error("Test.Error", "Test message");

        // Act & Assert
        error1.Should().Be(error2);
        (error1 == error2).Should().BeTrue();
    }

    [Fact]
    public void Error_Equality_DifferentCode_ReturnsFalse()
    {
        // Arrange
        var error1 = new Error("Test.Error1", "Test message");
        var error2 = new Error("Test.Error2", "Test message");

        // Act & Assert
        error1.Should().NotBe(error2);
        (error1 == error2).Should().BeFalse();
    }

    [Fact]
    public void Error_None_IsDefaultError()
    {
        // Act & Assert
        Error.None.Code.Should().Be("");
        Error.None.Message.Should().Be("");
    }
}