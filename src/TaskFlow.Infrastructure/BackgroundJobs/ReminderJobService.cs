using Hangfire;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Abstractions.BackgroundJobs;
using TaskFlow.Application.Abstractions.Services;

namespace TaskFlow.Infrastructure.BackgroundJobs;

internal sealed class ReminderJobService : IReminderJobService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IEmailService _emailService;
    private readonly ILogger<ReminderJobService> _logger;

    public ReminderJobService(
        IBackgroundJobClient backgroundJobClient,
        IEmailService emailService,
        ILogger<ReminderJobService> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _emailService = emailService;
        _logger = logger;
    }

    public Task ScheduleTaskReminderAsync(TaskReminderPayload payload, CancellationToken cancellationToken = default)
    {
        var jobId = $"task-reminder-{payload.TaskId}-{payload.ReminderType}";
        
        _backgroundJobClient.Schedule(
            () => SendTaskReminderAsync(payload, CancellationToken.None),
            payload.DueDate.Subtract(GetReminderOffset(payload.ReminderType)));

        _logger.LogInformation("Scheduled {ReminderType} reminder for task {TaskId} at {ScheduledTime}",
            payload.ReminderType, payload.TaskId, payload.DueDate.Subtract(GetReminderOffset(payload.ReminderType)));

        return Task.CompletedTask;
    }

    public Task ScheduleTaskRemindersAsync(
        Guid taskId,
        Guid organizationId,
        Guid assigneeUserId,
        string taskTitle,
        DateTime dueDate,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        
        if (dueDate > now.AddHours(24))
        {
            ScheduleTaskReminderAsync(new TaskReminderPayload(
                taskId, organizationId, assigneeUserId, taskTitle, dueDate, ReminderType.TwentyFourHours), cancellationToken);
        }

        if (dueDate > now.AddHours(1))
        {
            ScheduleTaskReminderAsync(new TaskReminderPayload(
                taskId, organizationId, assigneeUserId, taskTitle, dueDate, ReminderType.OneHour), cancellationToken);
        }

        if (dueDate > now)
        {
            ScheduleTaskReminderAsync(new TaskReminderPayload(
                taskId, organizationId, assigneeUserId, taskTitle, dueDate, ReminderType.Overdue), cancellationToken);
        }

        return Task.CompletedTask;
    }

    public Task CancelTaskRemindersAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        foreach (ReminderType type in Enum.GetValues(typeof(ReminderType)))
        {
            var jobId = $"task-reminder-{taskId}-{type}";
            _backgroundJobClient.Delete(jobId);
            _logger.LogInformation("Cancelled reminder job {JobId}", jobId);
        }

        return Task.CompletedTask;
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [60, 300, 900])]
    public async Task SendTaskReminderAsync(TaskReminderPayload payload, CancellationToken cancellationToken)
    {
        try
        {
            var subject = GetReminderSubject(payload.ReminderType, payload.TaskTitle);
            var body = GetReminderBody(payload.ReminderType, payload.TaskTitle, payload.DueDate);

            await _emailService.SendEmailAsync(payload.AssigneeUserId, subject, body, cancellationToken);
            
            _logger.LogInformation("Sent {ReminderType} reminder for task {TaskId} to user {UserId}",
                payload.ReminderType, payload.TaskId, payload.AssigneeUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send reminder for task {TaskId}", payload.TaskId);
            throw;
        }
    }

    private static TimeSpan GetReminderOffset(ReminderType type) => type switch
    {
        ReminderType.TwentyFourHours => TimeSpan.FromHours(24),
        ReminderType.OneHour => TimeSpan.FromHours(1),
        ReminderType.Overdue => TimeSpan.Zero,
        _ => TimeSpan.FromHours(24)
    };

    private static string GetReminderSubject(ReminderType type, string taskTitle) => type switch
    {
        ReminderType.TwentyFourHours => $"Reminder: Task due in 24 hours - {taskTitle}",
        ReminderType.OneHour => $"Urgent: Task due in 1 hour - {taskTitle}",
        ReminderType.Overdue => $"Overdue: Task past due - {taskTitle}",
        _ => $"Task Reminder - {taskTitle}"
    };

    private static string GetReminderBody(ReminderType type, string taskTitle, DateTime dueDate) => type switch
    {
        ReminderType.TwentyFourHours => $"Your task \"{taskTitle}\" is due in 24 hours (at {dueDate:yyyy-MM-dd HH:mm} UTC).",
        ReminderType.OneHour => $"Your task \"{taskTitle}\" is due in 1 hour (at {dueDate:yyyy-MM-dd HH:mm} UTC).",
        ReminderType.Overdue => $"Your task \"{taskTitle}\" was due at {dueDate:yyyy-MM-dd HH:mm} UTC and is now overdue.",
        _ => $"Reminder for task \"{taskTitle}\" due at {dueDate:yyyy-MM-dd HH:mm} UTC."
    };
}