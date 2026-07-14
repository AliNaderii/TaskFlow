using FluentValidation;
using TaskFlow.Domain.Constants;

namespace TaskFlow.Application.Organizations.Commands.UpdateOrganization;

public sealed class UpdateOrganizationCommandValidator 
    : AbstractValidator<UpdateOrganizationCommand>
{
    public UpdateOrganizationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty();
    }
}