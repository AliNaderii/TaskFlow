namespace TaskFlow.Application.Comments.Queries.GetCommentsByTaskId;

public sealed record CommentDto(
    Guid Id,
    Guid AuthorUserId,
    string Content,
    DateTime CreatedAt,
    DateTime? UpdatedAt);