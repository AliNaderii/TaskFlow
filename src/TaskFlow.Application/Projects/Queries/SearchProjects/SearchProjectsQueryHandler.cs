using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Application.Common;
using TaskFlow.Application.Projects.Queries.GetProjectById;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Projects.Queries.SearchProjects;

internal sealed class SearchProjectsQueryHandler
    : IQueryHandler<SearchProjectsQuery, PagedResult<ProjectDto>>
{
    private readonly IProjectRepository _projectRepository;

    public SearchProjectsQueryHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<Result<PagedResult<ProjectDto>>> Handle(
        SearchProjectsQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var (projects, totalCount) = await _projectRepository.SearchAsync(
            request.Keyword,
            request.IsArchived,
            page,
            pageSize,
            request.SortBy,
            request.SortDirection,
            cancellationToken);

        var items = projects
            .Select(p => new ProjectDto(
                p.Id,
                p.OrganizationId,
                p.Name.Value,
                p.Description?.Value,
                p.CreatedAt,
                p.UpdatedAt,
                p.ArchivedAt))
            .ToList();

        var pagedResult = PagedResult<ProjectDto>.Create(items, page, pageSize, totalCount);

        return Result<PagedResult<ProjectDto>>.Success(pagedResult);
    }
}