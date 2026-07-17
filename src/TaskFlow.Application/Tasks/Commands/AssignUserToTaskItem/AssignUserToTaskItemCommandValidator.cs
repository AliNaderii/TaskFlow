using FluentValidation;

namespace TaskFlow.Application.Tasks.Commands.AssignUserToTaskItem;

public sealed class AssignUserToTaskCommandValidator
    : AbstractValidator<AssignUserToTaskItemCommand>
{
    public AssignUserToTaskCommandValidator()
    {
        RuleFor(x => x.TaskItemId)
            .NotEmpty();

        RuleFor(x => x.AssigneeUserId)
            .NotEmpty();
    }
}