using TaskFlow.Domain.Enums;

namespace TaskFlow.Api.Contracts.Tasks;

public sealed record SearchTaskItemsRequest(
    Guid? ProjectId = null,
    string? Keyword = null,
    TaskItemStatus? Status = null,
    TaskItemPriority? Priority = null,
    Guid? AssigneeUserId = null,
    DateTime? DueDateFrom = null,
    DateTime? DueDateTo = null,
    bool? IsArchived = null,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDirection = null);