namespace TaskFlow.Application.Abstractions.BackgroundJobs;

public enum ReminderType
{
    TwentyFourHours = 1,
    OneHour = 2,
    Overdue = 3
}

public sealed record TaskReminderPayload(
    Guid TaskId,
    Guid OrganizationId,
    Guid AssigneeUserId,
    string TaskTitle,
    DateTime DueDate,
    ReminderType ReminderType);

public sealed record InvitationCleanupPayload(
    Guid InvitationId,
    Guid OrganizationId);

public sealed record NotificationCleanupPayload(
    Guid OrganizationId,
    DateTime OlderThan);

public sealed record StaleTaskReminderPayload(
    Guid TaskId,
    Guid OrganizationId,
    Guid AssigneeUserId,
    string TaskTitle,
    int DaysInProgress);