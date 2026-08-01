namespace TaskFlow.Api.Contracts.Comments;

public sealed record SearchCommentsRequest(
    Guid? TaskId = null,
    string? Keyword = null,
    Guid? AuthorUserId = null,
    DateTime? CreatedAtFrom = null,
    DateTime? CreatedAtTo = null,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDirection = null);