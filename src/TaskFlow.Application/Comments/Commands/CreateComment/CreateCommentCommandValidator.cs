using FluentValidation;

namespace TaskFlow.Application.Comments.Commands.CreateComment;

public sealed class CreateCommentCommandValidator
    : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty();

        RuleFor(x => x.Content)
            .NotEmpty();
    }
}
