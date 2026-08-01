using TaskFlow.Application.Abstractions.BackgroundJobs;

namespace TaskFlow.Application.Abstractions.BackgroundJobs;

public interface IReminderJobService
{
    Task ScheduleTaskReminderAsync(
        TaskReminderPayload payload, 
        CancellationToken cancellationToken = default);
    Task ScheduleTaskRemindersAsync(
        Guid taskId, 
        Guid organizationId, 
        Guid assigneeUserId, 
        string taskTitle, 
        DateTime dueDate, 
        CancellationToken cancellationToken = default);
    Task CancelTaskRemindersAsync(
        Guid taskId, 
        CancellationToken cancellationToken = default);
}

public interface ICleanupJobService
{
    Task ScheduleInvitationCleanupAsync(
        InvitationCleanupPayload payload, 
        CancellationToken cancellationToken = default);
    Task ScheduleNotificationCleanupAsync(
        NotificationCleanupPayload payload, 
        CancellationToken cancellationToken = default);
    Task EnqueueExpiredInvitationCleanupAsync(
        CancellationToken cancellationToken = default);
    Task EnqueueOldNotificationCleanupAsync(
        CancellationToken cancellationToken = default);
}

public interface IRecurringJobService
{
    void ConfigureRecurringJobs();
    Task EnqueueStaleTaskRemindersAsync(
        CancellationToken cancellationToken = default);
}