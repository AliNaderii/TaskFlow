using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Abstractions.BackgroundJobs;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.BackgroundJobs;

internal sealed class CleanupJobService : ICleanupJobService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CleanupJobService> _logger;

    public CleanupJobService(
        IBackgroundJobClient backgroundJobClient,
        IRecurringJobManager recurringJobManager,
        ApplicationDbContext context,
        ILogger<CleanupJobService> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
        _context = context;
        _logger = logger;
    }

    public Task ScheduleInvitationCleanupAsync(
        InvitationCleanupPayload payload, 
        CancellationToken cancellationToken = default)
    {
        var jobId = $"invitation-cleanup-{payload.InvitationId}";
        
        _backgroundJobClient.Schedule(
            () => CleanupExpiredInvitationAsync(
                payload.InvitationId, 
                CancellationToken.None),
            TimeSpan.FromDays(7)); // Run 7 days after invitation creation

        _logger.LogInformation(
            "Scheduled invitation cleanup for invitation {InvitationId}", 
            payload.InvitationId);

        return Task.CompletedTask;
    }

    public Task ScheduleNotificationCleanupAsync(
        NotificationCleanupPayload payload, 
        CancellationToken cancellationToken = default)
    {
        var jobId = $"notification-cleanup-{payload.OrganizationId}-{payload.OlderThan:yyyyMMdd}";
        
        _backgroundJobClient.Schedule(
            () => CleanupOldNotificationsAsync(
                payload.OrganizationId, 
                payload.OlderThan, 
                CancellationToken.None),
            TimeSpan.FromDays(1));

        _logger.LogInformation(
            "Scheduled notification cleanup for organization {OrganizationId} older than {Date}",
            payload.OrganizationId, 
            payload.OlderThan);

        return Task.CompletedTask;
    }

    public Task EnqueueExpiredInvitationCleanupAsync(CancellationToken cancellationToken = default)
    {
        _backgroundJobClient.Enqueue(
            () => CleanupAllExpiredInvitationsAsync(CancellationToken.None));

        _logger.LogInformation("Enqueued expired invitation cleanup job");

        return Task.CompletedTask;
    }

    public Task EnqueueOldNotificationCleanupAsync(CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-30);

        _backgroundJobClient.Enqueue(
            () => CleanupAllOldNotificationsAsync(cutoffDate, CancellationToken.None));

        _logger.LogInformation(
            "Enqueued old notification cleanup job (older than {Date})", 
            cutoffDate);
            
        return Task.CompletedTask;
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [60, 300, 900])]
    public async Task CleanupExpiredInvitationAsync(
        Guid invitationId, 
        CancellationToken cancellationToken)
    {
        try
        {
            var invitation = await _context.Invitations
                .FirstOrDefaultAsync(i => i.Id == invitationId, cancellationToken);

            if (
                invitation != null 
                && invitation.Status == Domain.Enums.InvitationStatus.Pending 
                && invitation.ExpiresAt < DateTime.UtcNow)
            {
                invitation.Expire();
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation(
                    "Marked invitation {InvitationId} as expired", 
                    invitationId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, 
                "Failed to cleanup invitation {InvitationId}", 
                invitationId);
            throw;
        }
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [60, 300, 900])]
    public async Task CleanupAllExpiredInvitationsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var expiredInvitations = await _context.Invitations
                .Where(
                    i => i.Status == Domain.Enums.InvitationStatus.Pending 
                    && i.ExpiresAt < DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            foreach (var invitation in expiredInvitations)
            {
                invitation.Expire();
            }

            if (expiredInvitations.Count > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation(
                    "Marked {Count} invitations as expired", 
                    expiredInvitations.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, 
                "Failed to cleanup expired invitations");
            throw;
        }
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [60, 300, 900])]
    public async Task CleanupOldNotificationsAsync(
        Guid organizationId, 
        DateTime olderThan, 
        CancellationToken cancellationToken)
    {
        try
        {
            var oldNotifications = await _context.Notifications
                .Where(
                    n => n.OrganizationId == organizationId 
                    && n.CreatedAt < olderThan 
                    && n.IsRead)
                .ToListAsync(cancellationToken);

            _context.Notifications.RemoveRange(oldNotifications);

            if (oldNotifications.Count > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation(
                    "Deleted {Count} old notifications for organization {OrganizationId}", 
                    oldNotifications.Count, organizationId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, 
                "Failed to cleanup old notifications for organization {OrganizationId}", 
                organizationId);
            throw;
        }
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [60, 300, 900])]
    public async Task CleanupAllOldNotificationsAsync(
        DateTime olderThan, 
        CancellationToken cancellationToken)
    {
        try
        {
            var oldNotifications = await _context.Notifications
                .Where(n => n.CreatedAt < olderThan && n.IsRead)
                .ToListAsync(cancellationToken);

            _context.Notifications.RemoveRange(oldNotifications);

            if (oldNotifications.Count > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation(
                    "Deleted {Count} old notifications across all organizations", 
                    oldNotifications.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup all old notifications");
            throw;
        }
    }
}