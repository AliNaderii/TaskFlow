using MediatR;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Events;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Errors;

namespace TaskFlow.Application.Events.Handlers;

internal sealed class CommentCreatedEventHandler : INotificationHandler<CommentCreatedEvent>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITaskItemRepository _taskItemRepository;
    private readonly ICommentRepository _commentRepository;

    public CommentCreatedEventHandler(
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork,
        ITaskItemRepository taskItemRepository,
        ICommentRepository commentRepository)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _taskItemRepository = taskItemRepository;
        _commentRepository = commentRepository;
    }

    public async Task Handle(CommentCreatedEvent notification, CancellationToken cancellationToken)
    {
        var task = await _taskItemRepository.GetByIdAsync(notification.TaskItemId, cancellationToken);
        if (task is null)
        {
            return;
        }

        var comment = await _commentRepository.GetByIdAsync(notification.CommentId, cancellationToken);
        if (comment is null)
        {
            return;
        }

        var recipientIds = new List<Guid>();
        if (task.CreatorUserId != notification.AuthorUserId)
        {
            recipientIds.Add(task.CreatorUserId);
        }
        if (task.AssigneeUserId.HasValue && task.AssigneeUserId.Value != notification.AuthorUserId && task.AssigneeUserId.Value != task.CreatorUserId)
        {
            recipientIds.Add(task.AssigneeUserId.Value);
        }

        foreach (var recipientId in recipientIds)
        {
            var notificationResult = Notification.Create(
                task.OrganizationId,
                recipientId,
                NotificationType.CommentCreated,
                "New Comment",
                $"{comment.Author.DisplayName} commented on task '{task.Title.Value}'",
                notification.TaskItemId);

            if (notificationResult.IsSuccess)
            {
                await _notificationRepository.AddAsync(notificationResult.Value, cancellationToken);
            }
        }

        if (recipientIds.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
