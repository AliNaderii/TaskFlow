using FluentValidation;

namespace TaskFlow.Application.Tasks.Commands.UnassignUserFromTaskItem;

public sealed class UnassignUserFromTaskItemCommandValidator
    : AbstractValidator<UnassignUserFromTaskItemCommand>
{
    public UnassignUserFromTaskItemCommandValidator()
    {
        RuleFor(x => x.TaskItemId)
            .NotEmpty();
    }
}