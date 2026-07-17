using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Application.Tasks.Commands.ArchiveTaskItem;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Errors;

public sealed class ArchiveTaskItemCommandHandler
    : ICommandHandler<ArchiveTaskItemCommand>
{
    private readonly ITaskItemRepository _taskItemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ArchiveTaskItemCommandHandler(
        ITaskItemRepository taskItemRepository,
        IUnitOfWork unitOfWork)
    {
        _taskItemRepository = taskItemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseResult> Handle(
        ArchiveTaskItemCommand request,
        CancellationToken cancellationToken)
    {
        var taskItem = await _taskItemRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (taskItem is null)
        {
            return BaseResult.Failure(TaskItemErrors.NotFound);
        }

        var result = taskItem.Archive();

        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BaseResult.Success();
    }
}

