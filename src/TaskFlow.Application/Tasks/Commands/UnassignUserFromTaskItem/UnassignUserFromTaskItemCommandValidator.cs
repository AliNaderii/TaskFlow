using FluentValidation;

namespace TaskFlow.Application.Tasks.Commands.UnassignUserFromTaskItem;

public sealed class UnassignUserFromTaskCommandValidator
    : AbstractValidator<UnassignUserFromTaskItemCommand>
{
    public UnassignUserFromTaskCommandValidator()
    {
        RuleFor(x => x.TaskItemId)
            .NotEmpty();
    }
}