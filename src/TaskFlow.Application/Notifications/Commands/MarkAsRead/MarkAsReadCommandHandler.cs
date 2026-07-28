using MediatR;
using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Application.Abstractions.Authentication;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Errors;

namespace TaskFlow.Application.Notifications.Commands.MarkAsRead;

internal sealed class MarkAsReadCommandHandler
    : ICommandHandler<MarkAsReadCommand, Unit>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUser _currentUser;

    public MarkAsReadCommandHandler(
        INotificationRepository notificationRepository,
        ICurrentUser currentUser)
    {
        _notificationRepository = notificationRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<Unit>> Handle(
        MarkAsReadCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.Id.HasValue)
        {
            return Result<Unit>.Failure(
                new Error("auth.user_not_found", "User not authenticated."));
        }

        var notification = await _notificationRepository.GetByIdAsync(
            request.NotificationId,
            cancellationToken);

        if (notification is null)
        {
            return Result<Unit>.Failure(NotificationErrors.NotFound);
        }

        if (notification.UserId != _currentUser.Id.Value)
        {
            return Result<Unit>.Failure(
                new Error("notification.unauthorized", "You are not authorized to mark this notification as read."));
        }

        var result = notification.MarkAsRead();

        if (result.IsFailure)
        {
            return Result<Unit>.Failure(result.Error);
        }

        await _notificationRepository.UpdateAsync(notification, cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}