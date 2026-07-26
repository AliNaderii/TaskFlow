using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.MultiTenancy;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Application.Abstractions.Authentication;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Tasks.Commands.CreateTaskItem;

public sealed class CreateTaskItemCommandHandler
    : ICommandHandler<CreateTaskItemCommand, Guid>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ITaskItemRepository _taskItemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    public CreateTaskItemCommandHandler(
        IProjectRepository projectRepository,
        ITaskItemRepository taskItemRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _projectRepository = projectRepository;
        _taskItemRepository = taskItemRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(
        CreateTaskItemCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentTenant.OrganizationId.HasValue)
        {
            return Result<Guid>.Failure(TenantErrors.NotFound);
        }

        if (!_currentUser.UserId.HasValue)
        {
            return Result<Guid>.Failure(new Error("auth.user_not_found", "User not authenticated."));
        }

        var project = await _projectRepository.GetByIdAsync(
            request.ProjectId,
            cancellationToken);

        if (project is null)
        {
            return Result<Guid>.Failure(ProjectErrors.NotFound);
        }

        var titleResult = TaskItemTitle.Create(request.Title);

        if (titleResult.IsFailure)
        {
            return Result<Guid>.Failure(titleResult.Error);
        }

        var descriptionResult = TaskItemDescription.Create(request.Description);

        if (descriptionResult.IsFailure)
        {
            return Result<Guid>.Failure(descriptionResult.Error);
        }

        var taskResult = TaskItem.Create(
            _currentTenant.OrganizationId.Value,
            request.ProjectId,
            _currentUser.UserId.Value,
            titleResult.Value,
            descriptionResult.Value,
            request.Priority,
            request.DueDate,
            request.AssigneeUserId);

        if (taskResult.IsFailure)
        {
            return Result<Guid>.Failure(taskResult.Error);
        }

        await _taskItemRepository.AddAsync(
            taskResult.Value,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(taskResult.Value.Id);
    }
}
