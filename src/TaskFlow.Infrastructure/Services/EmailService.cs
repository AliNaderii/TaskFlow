using TaskFlow.Application.Abstractions.Services;
using TaskFlow.Domain.Common;

namespace TaskFlow.Infrastructure.Services;

internal sealed class EmailService : IEmailService
{
    public Task<BaseResult> SendInvitationAsync(
        string toEmail,
        string organizationName,
        string invitedByName,
        string invitationLink,
        DateTime expiresAt,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[EMAIL] Sending invitation to: {toEmail}");
        Console.WriteLine($"[EMAIL] Organization: {organizationName}");
        Console.WriteLine($"[EMAIL] Invited by: {invitedByName}");
        Console.WriteLine($"[EMAIL] Invitation link: {invitationLink}");
        Console.WriteLine($"[EMAIL] Expires at: {expiresAt:yyyy-MM-dd HH:mm:ss} UTC");

        return Task.FromResult(BaseResult.Success());
    }

    public Task<BaseResult> SendEmailAsync(
        Guid userId,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[EMAIL] Sending email to user: {userId}");
        Console.WriteLine($"[EMAIL] Subject: {subject}");
        Console.WriteLine($"[EMAIL] Body: {body}");

        return Task.FromResult(BaseResult.Success());
    }
}
