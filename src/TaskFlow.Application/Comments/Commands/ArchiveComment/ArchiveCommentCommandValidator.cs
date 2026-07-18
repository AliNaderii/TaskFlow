using FluentValidation;

namespace TaskFlow.Application.Comments.Commands.ArchiveComment;

public sealed class ArchiveCommentCommandValidator
    : AbstractValidator<ArchiveCommentCommand>
{
    public ArchiveCommentCommandValidator()
    {
        RuleFor(x => x.CommentId)
            .NotEmpty();
    }
}