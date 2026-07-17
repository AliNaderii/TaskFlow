using FluentValidation;

namespace TaskFlow.Application.Tasks.Commands.ChangeTaskItemStatus;

public sealed class ChangeTaskItemStatusCommandValidator
    : AbstractValidator<ChangeTaskItemStatusCommand>
{
    public ChangeTaskItemStatusCommandValidator()
    {
        RuleFor(x => x.TaskItemId)
            .NotEmpty();

        RuleFor(x => x.Status)
            .IsInEnum();
    }
}