using TaskFlow.Domain.Common;
using TaskFlow.Domain.Errors;
using TaskFlow.Application.Authentication.Login;
using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Authentication;
using TaskFlow.Application.Abstractions.MultiTenancy;

namespace TaskFlow.Application.Authentication.RefreshToken;

public sealed class RefreshTokenCommandHandler
    : ICommandHandler<RefreshTokenCommand, LoginResponse>
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IJwtTokenProvider _jwtTokenProvider;
    private readonly ICurrentTenant _currentTenant;

    public RefreshTokenCommandHandler(
        IRefreshTokenService refreshTokenService,
        IJwtTokenProvider jwtTokenProvider,
        ICurrentTenant currentTenant)
    {
        _refreshTokenService = refreshTokenService;
        _jwtTokenProvider = jwtTokenProvider;
        _currentTenant = currentTenant;
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

        var organizationId = storedToken.OrganizationId == Guid.Empty
            ? _currentTenant.OrganizationId ?? Guid.Empty
            : storedToken.OrganizationId;

        var newRefreshToken = await _refreshTokenService.CreateAsync(
            storedToken.UserId,
            storedToken.Email,
            organizationId,
            cancellationToken);

        return Result<LoginResponse>.Success(
            new LoginResponse(
                storedToken.UserId,
                accessToken,
                newRefreshToken.Token));
    }
}