using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Errors;

namespace TaskFlow.Application.Tasks.Queries.GetTaskItemById;

public sealed class GetTaskItemByIdQueryHandler :
    IQueryHandler<GetTaskItemByIdQuery, TaskItemDto>
{
    private readonly ITaskItemRepository _taskItemRepository;

    public GetTaskItemByIdQueryHandler(
        ITaskItemRepository taskItemRepository)
    {
        _taskItemRepository = taskItemRepository;
    }

    public async Task<Result<TaskItemDto>> Handle(
        GetTaskItemByIdQuery request,
        CancellationToken cancellationToken)
    {
        var taskItem = await _taskItemRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (taskItem is null)
        {
            return Result<TaskItemDto>.Failure(
                TaskItemErrors.NotFound);
        }

        return Result<TaskItemDto>.Success(
            new TaskItemDto(
                taskItem.Id,
                taskItem.ProjectId,
                taskItem.CreatorUserId,
                taskItem.AssigneeUserId,
                taskItem.Title.Value,
                taskItem.Description?.Value,
                taskItem.Status,
                taskItem.Priority,
                taskItem.DueDate,
                taskItem.IsArchived,
                taskItem.CreatedAt,
                taskItem.UpdatedAt,
                taskItem.ArchivedAt));
    }
}