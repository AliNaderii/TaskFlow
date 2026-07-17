using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Persistence;
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

    public CreateTaskItemCommandHandler(
        IProjectRepository projectRepository,
        ITaskItemRepository taskItemRepository,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _taskItemRepository = taskItemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateTaskItemCommand request,
        CancellationToken cancellationToken)
    {
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
            request.ProjectId,
            request.CreatorUserId,
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