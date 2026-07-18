namespace TaskFlow.Application.Comments.Queries.GetCommentById;

public sealed record CommentDto(
    Guid Id,
    Guid TaskId,
    Guid AuthorUserId,
    string Content,
    DateTime CreatedAt,
    DateTime? UpdatedAt);