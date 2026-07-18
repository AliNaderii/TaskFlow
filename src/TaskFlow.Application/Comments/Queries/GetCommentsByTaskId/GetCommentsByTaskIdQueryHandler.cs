using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Comments.Queries.GetCommentsByTaskId;

public sealed class GetCommentsByTaskIdQueryHandler
    : IQueryHandler<GetCommentsByTaskIdQuery, IReadOnlyList<CommentDto>>
{
    private readonly ICommentRepository _commentRepository;

    public GetCommentsByTaskIdQueryHandler(
        ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<Result<IReadOnlyList<CommentDto>>> Handle(
        GetCommentsByTaskIdQuery request,
        CancellationToken cancellationToken)
    {
        var comments = await _commentRepository.GetByTaskIdAsync(
            request.TaskId,
            cancellationToken);

        var response = comments
            .Select(comment => new CommentDto(
                comment.Id,
                comment.AuthorUserId,
                comment.Content.Value,
                comment.CreatedAt,
                comment.UpdatedAt))
            .ToList();

        return Result<IReadOnlyList<CommentDto>>.Success(response);
    }
}