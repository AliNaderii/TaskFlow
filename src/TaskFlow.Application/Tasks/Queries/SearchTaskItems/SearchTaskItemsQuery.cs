using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Common;
using TaskFlow.Application.Tasks.Queries.GetTaskItemById;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Tasks.Queries.SearchTaskItems;

public sealed record SearchTaskItemsQuery(
    Guid? ProjectId,
    string? Keyword,
    TaskItemStatus? Status,
    TaskItemPriority? Priority,
    Guid? AssigneeUserId,
    DateTime? DueDateFrom,
    DateTime? DueDateTo,
    bool? IsArchived,
    int Page,
    int PageSize,
    string? SortBy,
    string? SortDirection)
    : IQuery<PagedResult<TaskItemDto>>;