namespace TaskFlow.Api.Contracts.Projects;

public sealed record SearchProjectsRequest(
    string? Keyword = null,
    bool? IsArchived = null,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDirection = null);