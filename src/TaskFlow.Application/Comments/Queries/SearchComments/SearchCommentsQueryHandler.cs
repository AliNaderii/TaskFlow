using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Application.Common;
using TaskFlow.Application.Comments.Queries.GetCommentsByTaskId;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Comments.Queries.SearchComments;

internal sealed class SearchCommentsQueryHandler
    : IQueryHandler<SearchCommentsQuery, PagedResult<CommentDto>>
{
    private readonly ICommentRepository _commentRepository;

    public SearchCommentsQueryHandler(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<Result<PagedResult<CommentDto>>> Handle(
        SearchCommentsQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var (comments, totalCount) = await _commentRepository.SearchAsync(
            request.TaskId,
            request.Keyword,
            request.AuthorUserId,
            request.CreatedAtFrom,
            request.CreatedAtTo,
            page,
            pageSize,
            request.SortBy,
            request.SortDirection,
            cancellationToken);

        var items = comments
            .Select(c => new CommentDto(
                c.Id,
                c.AuthorUserId,
                c.Content.Value,
                c.CreatedAt,
                c.UpdatedAt))
            .ToList();

        var pagedResult = PagedResult<CommentDto>.Create(items, page, pageSize, totalCount);

        return Result<PagedResult<CommentDto>>.Success(pagedResult);
    }
}