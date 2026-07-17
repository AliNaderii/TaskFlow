using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Errors;

namespace TaskFlow.Application.Tasks.Commands.UnassignUserFromTaskItem;

public sealed class UnassignUserFromTaskCommandHandler
    : ICommandHandler<UnassignUserFromTaskItemCommand>
{
    private readonly ITaskItemRepository _taskItemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UnassignUserFromTaskCommandHandler(
        ITaskItemRepository taskItemRepository,
        IUnitOfWork unitOfWork)
    {
        _taskItemRepository = taskItemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseResult> Handle(
        UnassignUserFromTaskItemCommand request,
        CancellationToken cancellationToken)
    {
        var taskItem = await _taskItemRepository.GetByIdAsync(
            request.TaskItemId,
            cancellationToken);

        if (taskItem is null)
        {
            return BaseResult.Failure(TaskItemErrors.NotFound);
        }

        var result = taskItem.Unassign();

        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BaseResult.Success();
    }
}