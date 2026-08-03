using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Events;

namespace TaskFlow.Application.Events.Handlers;

public sealed class TaskCompletedEventHandler : IDomainEventHandler<TaskCompletedEvent>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITaskItemRepository _taskItemRepository;

    public TaskCompletedEventHandler(
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork,
        ITaskItemRepository taskItemRepository)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _taskItemRepository = taskItemRepository;
    }

    public async Task Handle(TaskCompletedEvent notification, CancellationToken cancellationToken)
    {
        var task = await _taskItemRepository.GetByIdAsync(notification.TaskItemId, cancellationToken);
        if (task is null)
        {
            return;
        }

        var notificationResult = Notification.Create(
            task.OrganizationId,
            task.CreatorUserId,
            NotificationType.TaskCompleted,
            "Task Completed",
            $"Task '{task.Title.Value}' has been marked as completed",
            notification.TaskItemId);

        if (notificationResult.IsSuccess)
        {
            await _notificationRepository.AddAsync(notificationResult.Value, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
