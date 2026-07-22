using TaskFlow.Domain.Common;
using TaskFlow.Domain.Errors;
using TaskFlow.Application.Authentication.Login;
using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Authentication;

namespace TaskFlow.Application.Authentication.RefreshToken;

public sealed class RefreshTokenCommandHandler
    : ICommandHandler<RefreshTokenCommand, LoginResponse>
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IJwtTokenProvider _jwtTokenProvider;

    public RefreshTokenCommandHandler(
        IRefreshTokenService refreshTokenService,
        IJwtTokenProvider jwtTokenProvider)
    {
        _refreshTokenService = refreshTokenService;
        _jwtTokenProvider = jwtTokenProvider;
    }

    public async Task<Result<LoginResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var storedToken = await _refreshTokenService.GetAsync(
            request.RefreshToken,
            cancellationToken);

        if (storedToken is null)
        {
            return Result<LoginResponse>.Failure(AuthenticationErrors.InvalidRefreshToken);
        }

        if (storedToken.ExpiresAt <= DateTime.UtcNow)
        {
            return Result<LoginResponse>.Failure(AuthenticationErrors.ExpiredRefreshToken);
        }

        await _refreshTokenService.RevokeAsync(
            request.RefreshToken,
            cancellationToken);


        var accessToken = _jwtTokenProvider.GenerateToken(
            storedToken.UserId,
            storedToken.Email);


        var newRefreshToken = await _refreshTokenService.CreateAsync(
            storedToken.UserId,
            storedToken.Email,
            cancellationToken);


        return Result<LoginResponse>.Success(
            new LoginResponse(
                storedToken.UserId,
                accessToken,
                newRefreshToken.Token));
    }
}