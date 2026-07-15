using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Errors;

namespace TaskFlow.Application.Projects.Queries.GetProjectById;

public sealed class GetProjectByIdQueryHandler :
    IQueryHandler<GetProjectByIdQuery, ProjectDto>
{
    private readonly IProjectRepository _projectRepository;

    public GetProjectByIdQueryHandler(
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
    }

    public async Task<Result<ProjectDto>> Handle(
        GetProjectByIdQuery request, 
        CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (project is null)
        {
            return Result<ProjectDto>.Failure(ProjectErrors.NotFound);
        }

        return Result<ProjectDto>.Success(
            new ProjectDto(
                project.Id,
                project.OrganizationId,
                project.Name.Value,
                project.Description?.Value,
                project.CreatedAt,
                project.UpdatedAt,
                project.ArchivedAt));
    }
}