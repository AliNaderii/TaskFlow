using TaskFlow.Application.Abstractions.Messaging;

namespace TaskFlow.Application.Comments.Queries.GetCommentsByTaskId;

public sealed record GetCommentsByTaskIdQuery(
    Guid TaskId)
    : IQuery<IReadOnlyList<CommentDto>>;