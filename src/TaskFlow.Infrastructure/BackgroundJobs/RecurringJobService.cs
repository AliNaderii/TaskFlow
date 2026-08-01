using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Abstractions.BackgroundJobs;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Infrastructure.BackgroundJobs;

internal sealed class RecurringJobService : IRecurringJobService
{
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RecurringJobService> _logger;

    public RecurringJobService(
        IRecurringJobManager recurringJobManager,
        IBackgroundJobClient backgroundJobClient,
        ApplicationDbContext context,
        ILogger<RecurringJobService> logger)
    {
        _recurringJobManager = recurringJobManager;
        _backgroundJobClient = backgroundJobClient;
        _context = context;
        _logger = logger;
    }

    public void ConfigureRecurringJobs()
    {
        // Daily at 02:00 UTC - Cleanup expired invitations
        _recurringJobManager.AddOrUpdate<ICleanupJobService>(
            "cleanup-expired-invitations",
            service => service.EnqueueExpiredInvitationCleanupAsync(CancellationToken.None),
            "0 2 * * *",
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });

        // Daily at 03:00 UTC - Cleanup old notifications (30+ days, read)
        _recurringJobManager.AddOrUpdate<ICleanupJobService>(
            "cleanup-old-notifications",
            service => service.EnqueueOldNotificationCleanupAsync(CancellationToken.None),
            "0 3 * * *",
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });

        // Daily at 04:00 UTC - Check for stale tasks (in progress > 7 days)
        _recurringJobManager.AddOrUpdate<IRecurringJobService>(
            "stale-task-reminders",
            service => service.EnqueueStaleTaskRemindersAsync(CancellationToken.None),
            "0 4 * * *",
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });

        _logger.LogInformation("Configured recurring background jobs");
    }

    public Task EnqueueStaleTaskRemindersAsync(CancellationToken cancellationToken = default)
    {
        _backgroundJobClient.Enqueue(() => ProcessStaleTaskRemindersAsync(CancellationToken.None));
        _logger.LogInformation("Enqueued stale task reminders job");
        return Task.CompletedTask;
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [60, 300, 900])]
    public async Task ProcessStaleTaskRemindersAsync(CancellationToken cancellationToken)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-7);
            
            var staleTasks = await _context.TaskItems
                .Where(t => t.Status == TaskItemStatus.InProgress 
                    && t.UpdatedAt < cutoffDate
                    && t.AssigneeUserId != null)
                .ToListAsync(cancellationToken);

            foreach (var task in staleTasks)
            {
                if (task.AssigneeUserId.HasValue)
                {
                    _backgroundJobClient.Enqueue<IReminderJobService>(
                        s => s.ScheduleTaskReminderAsync(
                            new TaskReminderPayload(
                                task.Id,
                                task.OrganizationId,
                                task.AssigneeUserId.Value,
                                task.Title.Value,
                                task.DueDate ?? DateTime.UtcNow.AddDays(1),
                                ReminderType.Overdue),
                            CancellationToken.None));
                }
            }

            _logger.LogInformation("Enqueued stale task reminders for {Count} tasks", staleTasks.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process stale task reminders");
            throw;
        }
    }
}
