using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.MultiTenancy;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Projects.Commands.CreateProject;

public sealed class CreateProjectCommandHandler
    : ICommandHandler<CreateProjectCommand, Guid>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;

    public CreateProjectCommandHandler(
        IProjectRepository projectRepository,
        IOrganizationRepository organizationRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant)
    {
        _projectRepository = projectRepository;
        _organizationRepository = organizationRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<Result<Guid>> Handle(
        CreateProjectCommand request, 
        CancellationToken cancellationToken)
    {
        if (!_currentTenant.OrganizationId.HasValue)
        {
            return Result<Guid>.Failure(TenantErrors.NotFound);
        }

        var organizationId = _currentTenant.OrganizationId.Value;

        var organization = await _organizationRepository.GetByIdAsync(
            organizationId,
            cancellationToken);

        if (organization is null)
        {
            return Result<Guid>.Failure(OrganizationErrors.NotFound);
        }

        var projectNameResult = ProjectName.Create(request.Name);

        if (projectNameResult.IsFailure)
        {
            return Result<Guid>.Failure(projectNameResult.Error);
        }

        var projectDescriptionResult = ProjectDescription.Create(request.Description);

        if (projectDescriptionResult.IsFailure)
        {
            return Result<Guid>.Failure(projectDescriptionResult.Error);
        }

        var exists = await _projectRepository.ExistsByNameAsync(
                projectNameResult.Value,
                cancellationToken: cancellationToken);

        if (exists)
        {
            return Result<Guid>.Failure(ProjectErrors.AlreadyExists);
        }

        var createProjectResult = Project.Create(
            organizationId,
            projectNameResult.Value,
            projectDescriptionResult.Value);

        if (createProjectResult.IsFailure)
        {
            return Result<Guid>.Failure(createProjectResult.Error);
        }

        await _projectRepository.AddAsync(
            createProjectResult.Value,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(createProjectResult.Value.Id);
    }
}