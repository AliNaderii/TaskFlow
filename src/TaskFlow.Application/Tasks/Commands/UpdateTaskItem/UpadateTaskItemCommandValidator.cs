using FluentValidation;
using TaskFlow.Domain.Constants;

namespace TaskFlow.Application.Tasks.Commands.UpdateTaskItem;

public sealed class UpdateTaskItemCommandValidator
    : AbstractValidator<UpdateTaskItemCommand>
{
    public UpdateTaskItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(TaskItemConstants.TitleMaxLength);

        RuleFor(x => x.Description)
            .MaximumLength(TaskItemConstants.DescriptionMaxLength);
    }
}