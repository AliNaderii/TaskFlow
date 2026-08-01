using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Common;
using TaskFlow.Application.Comments.Queries.GetCommentsByTaskId;

namespace TaskFlow.Application.Comments.Queries.SearchComments;

public sealed record SearchCommentsQuery(
    Guid? TaskId,
    string? Keyword,
    Guid? AuthorUserId,
    DateTime? CreatedAtFrom,
    DateTime? CreatedAtTo,
    int Page,
    int PageSize,
    string? SortBy,
    string? SortDirection)
    : IQuery<PagedResult<CommentDto>>;