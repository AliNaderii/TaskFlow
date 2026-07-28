using TaskFlow.Application.Abstractions.Messaging;

namespace TaskFlow.Application.Notifications.Queries.GetUnreadCount;

public sealed record GetUnreadCountQuery : IQuery<int>;