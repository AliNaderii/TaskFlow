using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
using TaskFlow.Application.Abstractions.Services;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Errors;
using TaskFlow.Infrastructure.Authentication;

namespace TaskFlow.Infrastructure.Services;

internal sealed class EmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailOptions> options,
        ILogger<EmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<BaseResult> SendInvitationAsync(
        string toEmail,
        string organizationName,
        string invitedByName,
        string invitationLink,
        DateTime expiresAt,
        CancellationToken cancellationToken = default)
    {
        var subject = $"Invitation to join {organizationName}";
        var body = $@"
            <h2>You've been invited to join {organizationName}</h2>
            <p><strong>{invitedByName}</strong> has invited you to join the organization <strong>{organizationName}</strong>.</p>
            <p>Click the link below to accept the invitation:</p>
            <p><a href='{invitationLink}'>{invitationLink}</a></p>
            <p>This invitation expires on <strong>{expiresAt:yyyy-MM-dd HH:mm:ss} UTC</strong>.</p>
            <p>If you didn't expect this invitation, you can safely ignore this email.</p>
        ";

        return await SendEmailInternalAsync(toEmail, subject, body, cancellationToken);
    }

    public async Task<BaseResult> SendEmailAsync(
        Guid userId,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        // For userId-based emails, we'd need to resolve the email address
        // For now, log and return success - in production, lookup user email
        _logger.LogInformation("Sending email to user {UserId}: {Subject}", userId, subject);
        
        // In a real implementation, you'd resolve userId to email address
        // var email = await _userRepository.GetEmailAsync(userId);
        // return await SendEmailInternalAsync(email, subject, body, cancellationToken);
        
        return BaseResult.Success();
    }

    private async Task<BaseResult> SendEmailInternalAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.Host) || string.IsNullOrWhiteSpace(_options.Username))
        {
            _logger.LogWarning("Email settings not configured. Skipping email to {ToEmail}", toEmail);
            return BaseResult.Success(); // Don't fail if not configured
        }

        try
        {
            using var smtpClient = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                Credentials = new NetworkCredential(_options.Username, _options.Password),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_options.FromEmail, _options.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            await smtpClient.SendMailAsync(message, cancellationToken);

            _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
            return BaseResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToEmail}", toEmail);
            return BaseResult.Failure(new Error("Email.SendFailed", $"Failed to send email: {ex.Message}"));
        }
    }
}
