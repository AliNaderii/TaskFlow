using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Application.Abstractions.Authentication;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Notifications.Queries.GetNotifications;

internal sealed class GetNotificationsQueryHandler
    : IQueryHandler<GetNotificationsQuery, IReadOnlyList<NotificationResponse>>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUser _currentUser;

    public GetNotificationsQueryHandler(
        INotificationRepository notificationRepository,
        ICurrentUser currentUser)
    {
        _notificationRepository = notificationRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<NotificationResponse>>> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.Id.HasValue)
        {
            return Result<IReadOnlyList<NotificationResponse>>.Failure(
                new Error("auth.user_not_found", "User not authenticated."));
        }

        var notifications = await _notificationRepository.GetByUserIdAsync(
            _currentUser.Id.Value,
            request.IsRead,
            request.Page,
            request.PageSize,
            cancellationToken);

        var response = notifications
            .Select(n => new NotificationResponse(
                n.Id,
                n.Type,
                n.Title,
                n.Message,
                n.RelatedEntityId,
                n.IsRead,
                n.ReadAt,
                n.CreatedAt))
            .ToList();

        return Result<IReadOnlyList<NotificationResponse>>.Success(response);
    }
}