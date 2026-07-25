using FluentValidation;

namespace TaskFlow.Application.Projects.Commands.CreateProject;

public sealed class CreateProjectCommandValidator
    : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty();
    }
}
