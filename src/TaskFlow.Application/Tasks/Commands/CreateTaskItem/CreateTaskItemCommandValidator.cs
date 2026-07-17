using FluentValidation;

namespace TaskFlow.Application.Tasks.Commands.CreateTaskItem;

public sealed class CreateTaskItemCommandValidator
    : AbstractValidator<CreateTaskItemCommand>
{
    public CreateTaskItemCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty();
        
        RuleFor(x => x.CreatorUserId)
            .NotEmpty();
    }
}