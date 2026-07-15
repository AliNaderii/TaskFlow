using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Projects.Commands.UpdateProject;

public sealed class UpdateProjectCommandHandler
    : ICommandHandler<UpdateProjectCommand>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProjectCommandHandler(
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseResult> Handle(
        UpdateProjectCommand request,
        CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (project is null)
        {
            return BaseResult.Failure(ProjectErrors.NotFound);
        }

        var nameResult = ProjectName.Create(request.Name);

        if (nameResult.IsFailure)
        {
            return BaseResult.Failure(nameResult.Error);
        }

        var descriptionResult = ProjectDescription.Create(request.Description);

        if (descriptionResult.IsFailure)
        {
            return BaseResult.Failure(descriptionResult.Error);
        }

        if (project.Name != nameResult.Value)
        {
            var exists = await _projectRepository.ExistsByNameAsync(
                project.OrganizationId,
                nameResult.Value,
                project.Id,
                cancellationToken);

            if (exists)
            {
                return BaseResult.Failure(ProjectErrors.AlreadyExists);
            }

            var renameResult = project.Rename(nameResult.Value);

            if (renameResult.IsFailure)
            {
                return renameResult;
            }
        }

        var changeDescriptionResult =
            project.ChangeDescription(descriptionResult.Value);

        if (changeDescriptionResult.IsFailure)
        {
            return changeDescriptionResult;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BaseResult.Success();
    }
}