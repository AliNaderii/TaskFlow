using FluentValidation;
using TaskFlow.Domain.Constants;

namespace TaskFlow.Application.Organizations.Commands.CreateOrganization;

public sealed class CreateOrganizationCommandValidator 
    : AbstractValidator<CreateOrganizationCommand>
{
    public CreateOrganizationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(OrganizationConstants.NameMinLength)
            .MaximumLength(OrganizationConstants.NameMaxLength);
    }
}