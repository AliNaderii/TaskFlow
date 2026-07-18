namespace TaskFlow.Api.Contracts.Comments;

public sealed record CreateCommentRequest(
    Guid AuthorUserId,
    string Content);