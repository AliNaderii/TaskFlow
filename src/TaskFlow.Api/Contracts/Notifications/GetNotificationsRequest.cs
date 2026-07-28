namespace TaskFlow.Api.Contracts.Notifications;

public sealed record GetNotificationsRequest(
    bool? IsRead,
    int Page = 1,
    int PageSize = 20);