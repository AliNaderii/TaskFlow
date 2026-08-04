using TaskFlow.Application.Abstractions.Authentication;
using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Authentication.Logout;

public sealed class LogoutCommandHandler
    : ICommandHandler<LogoutCommand>
{
    private readonly IRefreshTokenService _refreshTokenService;

    public LogoutCommandHandler(
        IRefreshTokenService refreshTokenService)
    {
        _refreshTokenService = refreshTokenService;
    }

    public async Task<BaseResult> Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        var revoked = await _refreshTokenService.RevokeAsync(
            request.RefreshToken,
            cancellationToken);

        if (!revoked)
        {
            return BaseResult.Failure(new Error(
                "Authentication.InvalidRefreshToken",
                "Invalid or already revoked refresh token."));
        }

        return BaseResult.Success();
    }
}
