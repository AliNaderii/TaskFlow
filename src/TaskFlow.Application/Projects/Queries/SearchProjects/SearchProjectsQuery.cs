using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Common;
using TaskFlow.Application.Projects.Queries.GetProjectById;

namespace TaskFlow.Application.Projects.Queries.SearchProjects;

public sealed record SearchProjectsQuery(
    string? Keyword,
    bool? IsArchived,
    int Page,
    int PageSize,
    string? SortBy,
    string? SortDirection)
    : IQuery<PagedResult<ProjectDto>>;
