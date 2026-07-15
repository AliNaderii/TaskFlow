using FluentValidation;

namespace TaskFlow.Application.Projects.Commands.UpdateProject;

public sealed class UpdateProjectCommandValidator
    : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty();
    }
}