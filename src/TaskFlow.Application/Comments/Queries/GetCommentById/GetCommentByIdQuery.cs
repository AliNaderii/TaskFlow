using TaskFlow.Application.Abstractions.Messaging;

namespace TaskFlow.Application.Comments.Queries.GetCommentById;

public sealed record GetCommentByIdQuery(
    Guid CommentId)
    : IQuery<CommentDto>;