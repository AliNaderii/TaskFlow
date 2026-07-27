using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Abstractions.Services;

public interface IEmailService
{
    Task<BaseResult> SendInvitationAsync(
        string toEmail,
        string organizationName,
        string invitedByName,
        string invitationLink,
        DateTime expiresAt,
        CancellationToken cancellationToken = default);
}
