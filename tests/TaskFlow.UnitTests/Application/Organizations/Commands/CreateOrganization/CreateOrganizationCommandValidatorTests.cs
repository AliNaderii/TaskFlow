using FluentAssertions;
using FluentValidation.TestHelper;
using TaskFlow.Application.Organizations.Commands.CreateOrganization;

namespace TaskFlow.UnitTests.Application.Organizations.Commands.CreateOrganization;

public class CreateOrganizationCommandValidatorTests
{
    private readonly CreateOrganizationCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidName_Passes()
    {
        // Arrange
        var command = new CreateOrganizationCommand("Valid Organization");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_EmptyName_Fails()
    {
        // Arrange
        var command = new CreateOrganizationCommand("");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WhitespaceName_Fails()
    {
        // Arrange
        var command = new CreateOrganizationCommand("   ");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameTooShort_Fails()
    {
        // Arrange
        var shortName = new string('a', 2); // Less than min length (3)
        var command = new CreateOrganizationCommand(shortName);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameTooLong_Fails()
    {
        // Arrange
        var longName = new string('a', 101); // More than max length
        var command = new CreateOrganizationCommand(longName);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameAtMinLength_Passes()
    {
        // Arrange
        var minName = new string('a', 3); // Min length
        var command = new CreateOrganizationCommand(minName);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_NameAtMaxLength_Passes()
    {
        // Arrange
        var maxName = new string('a', 100); // Max length
        var command = new CreateOrganizationCommand(maxName);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }
}