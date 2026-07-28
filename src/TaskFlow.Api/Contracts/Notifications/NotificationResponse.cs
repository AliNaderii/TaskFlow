namespace TaskFlow.Api.Contracts.Notifications;

public sealed record NotificationResponse(
    Guid Id,
    Guid OrganizationId,
    Guid UserId,
    string Type,
    string Title,
    string Message,
    bool IsRead,
    Guid? RelatedEntityId,
    DateTime CreatedAt);