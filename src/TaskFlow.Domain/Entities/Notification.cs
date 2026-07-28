using TaskFlow.Domain.Common;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Errors;

namespace TaskFlow.Domain.Entities;

public sealed class Notification : AuditableEntity, ITenantEntity
{
    public Guid OrganizationId { get; private set; }
    public Guid UserId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Title { get; private set; } = null!;
    public string Message { get; private set; } = null!;
    public Guid? RelatedEntityId { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public User User { get; private set; } = null!;

    private Notification() { }

    private Notification(
        Guid organizationId,
        Guid userId,
        NotificationType type,
        string title,
        string message,
        Guid? relatedEntityId = null)
    {
        OrganizationId = organizationId;
        UserId = userId;
        Type = type;
        Title = title;
        Message = message;
        RelatedEntityId = relatedEntityId;
        IsRead = false;
        ReadAt = null;
    }

    public static Result<Notification> Create(
        Guid organizationId,
        Guid userId,
        NotificationType type,
        string title,
        string message,
        Guid? relatedEntityId = null)
    {
        if (organizationId == Guid.Empty)
        {
            return Result<Notification>.Failure(NotificationErrors.InvalidOrganizationId);
        }

        if (userId == Guid.Empty)
        {
            return Result<Notification>.Failure(NotificationErrors.InvalidUserId);
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            return Result<Notification>.Failure(NotificationErrors.TitleRequired);
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            return Result<Notification>.Failure(NotificationErrors.MessageRequired);
        }

        var notification = new Notification(
            organizationId,
            userId,
            type,
            title.Trim(),
            message.Trim(),
            relatedEntityId);

        return Result<Notification>.Success(notification);
    }

    public BaseResult MarkAsRead()
    {
        if (IsRead)
        {
            return BaseResult.Failure(NotificationErrors.AlreadyRead);
        }

        IsRead = true;
        ReadAt = DateTime.UtcNow;

        return BaseResult.Success();
    }

    public BaseResult MarkAsUnread()
    {
        if (!IsRead)
        {
            return BaseResult.Failure(NotificationErrors.NotRead);
        }

        IsRead = false;
        ReadAt = null;

        return BaseResult.Success();
    }
}