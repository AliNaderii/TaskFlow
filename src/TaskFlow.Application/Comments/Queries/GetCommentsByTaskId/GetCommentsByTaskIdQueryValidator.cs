using FluentValidation;

namespace TaskFlow.Application.Comments.Queries.GetCommentsByTaskId;

public sealed class GetCommentsByTaskIdQueryValidator
    : AbstractValidator<GetCommentsByTaskIdQuery>
{
    public GetCommentsByTaskIdQueryValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty();
    }
}