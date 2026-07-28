using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Errors;

public static class NotificationErrors
{
    public static readonly Error InvalidOrganizationId =
        new(
            "Notification.InvalidOrganizationId",
            "OrganizationId is required.");

    public static readonly Error InvalidUserId =
        new(
            "Notification.InvalidUserId",
            "UserId is required.");

    public static readonly Error TitleRequired =
        new(
            "Notification.Title.Required",
            "Title is required.");

    public static readonly Error MessageRequired =
        new(
            "Notification.Message.Required",
            "Message is required.");

    public static readonly Error AlreadyRead =
        new(
            "Notification.AlreadyRead",
            "Notification is already read.");

    public static readonly Error NotRead =
        new(
            "Notification.NotRead",
            "Notification is not read.");

    public static readonly Error NotFound =
        new(
            "Notification.NotFound",
            "Notification does not exist.");
}