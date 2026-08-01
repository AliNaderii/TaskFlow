using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Application.Common;
using TaskFlow.Application.Tasks.Queries.GetTaskItemById;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Tasks.Queries.SearchTaskItems;

internal sealed class SearchTaskItemsQueryHandler
    : IQueryHandler<SearchTaskItemsQuery, PagedResult<TaskItemDto>>
{
    private readonly ITaskItemRepository _taskItemRepository;

    public SearchTaskItemsQueryHandler(ITaskItemRepository taskItemRepository)
    {
        _taskItemRepository = taskItemRepository;
    }

    public async Task<Result<PagedResult<TaskItemDto>>> Handle(
        SearchTaskItemsQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var (items, totalCount) = await _taskItemRepository.SearchAsync(
            request.ProjectId,
            request.Keyword,
            request.Status,
            request.Priority,
            request.AssigneeUserId,
            request.DueDateFrom,
            request.DueDateTo,
            request.IsArchived,
            page,
            pageSize,
            request.SortBy,
            request.SortDirection,
            cancellationToken);

        var dtos = items
            .Select(t => new TaskItemDto(
                t.Id,
                t.ProjectId,
                t.CreatorUserId,
                t.AssigneeUserId,
                t.Title.Value,
                t.Description?.Value,
                t.Status,
                t.Priority,
                t.DueDate,
                t.IsArchived,
                t.CreatedAt,
                t.UpdatedAt,
                t.ArchivedAt))
            .ToList();

        var pagedResult = PagedResult<TaskItemDto>.Create(dtos, page, pageSize, totalCount);

        return Result<PagedResult<TaskItemDto>>.Success(pagedResult);
    }
}