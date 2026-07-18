using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Errors;

namespace TaskFlow.Application.Comments.Queries.GetCommentById;

public sealed class GetCommentByIdQueryHandler
    : IQueryHandler<GetCommentByIdQuery, CommentDto>
{
    private readonly ICommentRepository _commentRepository;

    public GetCommentByIdQueryHandler(
        ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<Result<CommentDto>> Handle(
        GetCommentByIdQuery request,
        CancellationToken cancellationToken)
    {
        var comment = await _commentRepository.GetByIdAsync(
            request.CommentId,
            cancellationToken);

        if (comment is null)
        {
            return Result<CommentDto>.Failure(
                CommentErrors.NotFound);
        }

        var response = new CommentDto(
            comment.Id,
            comment.TaskId,
            comment.AuthorUserId,
            comment.Content.Value,
            comment.CreatedAt,
            comment.UpdatedAt);

        return Result<CommentDto>.Success(response);
    }
}