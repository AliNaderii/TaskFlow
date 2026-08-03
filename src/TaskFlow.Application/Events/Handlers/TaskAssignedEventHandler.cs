using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Events;

namespace TaskFlow.Application.Events.Handlers;

public sealed class TaskAssignedEventHandler : IDomainEventHandler<TaskAssignedEvent>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITaskItemRepository _taskItemRepository;

    public TaskAssignedEventHandler(
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork,
        ITaskItemRepository taskItemRepository)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _taskItemRepository = taskItemRepository;
    }

    public async Task Handle(TaskAssignedEvent notification, CancellationToken cancellationToken)
    {
        var task = await _taskItemRepository.GetByIdAsync(notification.TaskItemId, cancellationToken);
        if (task is null)
        {
            return;
        }

        var notificationResult = Notification.Create(
            task.OrganizationId,
            notification.AssigneeUserId,
            NotificationType.TaskAssigned,
            "Task Assigned",
            $"You have been assigned to task '{task.Title.Value}'",
            notification.TaskItemId);

        if (notificationResult.IsSuccess)
        {
            await _notificationRepository.AddAsync(notificationResult.Value, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
