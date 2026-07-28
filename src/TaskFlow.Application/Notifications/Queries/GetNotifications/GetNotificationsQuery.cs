using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Notifications.Queries.GetNotifications;

public sealed record GetNotificationsQuery(
    bool? IsRead = null,
    int Page = 1,
    int PageSize = 20)
    : IQuery<IReadOnlyList<NotificationResponse>>;

public sealed record NotificationResponse(
    Guid Id,
    NotificationType Type,
    string Title,
    string Message,
    Guid? RelatedEntityId,
    bool IsRead,
    DateTime? ReadAt,
    DateTime CreatedAt);