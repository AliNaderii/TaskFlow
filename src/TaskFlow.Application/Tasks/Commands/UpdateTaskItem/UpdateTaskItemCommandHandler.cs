using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Tasks.Commands.UpdateTaskItem;

public sealed class UpdateTaskItemCommandHandler
    : ICommandHandler<UpdateTaskItemCommand>
{
    private readonly ITaskItemRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTaskItemCommandHandler(
        ITaskItemRepository taskRepository,
        IUnitOfWork unitOfWork)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseResult> Handle(
        UpdateTaskItemCommand request,
        CancellationToken cancellationToken)
    {
        var taskItem = await _taskRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (taskItem is null)
        {
            return BaseResult.Failure(TaskItemErrors.NotFound);
        }

        var renameResult = taskItem.Rename(request.Title);

        if (renameResult.IsFailure)
        {
            return renameResult;
        }

        var descriptionResult = taskItem.ChangeDescription(request.Description);

        if (descriptionResult.IsFailure)
        {
            return descriptionResult;
        }

        taskItem.ChangePriority(request.Priority);
        taskItem.ChangeDueDate(request.DueDate);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BaseResult.Success();
    }
}