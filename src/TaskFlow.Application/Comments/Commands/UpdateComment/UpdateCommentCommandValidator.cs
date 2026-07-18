using FluentValidation;

namespace TaskFlow.Application.Comments.Commands.UpdateComment;

public sealed class UpdateCommentCommandValidator
    : AbstractValidator<UpdateCommentCommand>
{
    public UpdateCommentCommandValidator()
    {
        RuleFor(x => x.CommentId)
            .NotEmpty();

        RuleFor(x => x.Content)
            .NotEmpty();
    }
}