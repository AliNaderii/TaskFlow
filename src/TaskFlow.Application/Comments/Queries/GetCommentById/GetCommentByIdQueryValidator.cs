using FluentValidation;

namespace TaskFlow.Application.Comments.Queries.GetCommentById;

public sealed class GetCommentByIdQueryValidator
    : AbstractValidator<GetCommentByIdQuery>
{
    public GetCommentByIdQueryValidator()
    {
        RuleFor(x => x.CommentId)
            .NotEmpty();
    }
}