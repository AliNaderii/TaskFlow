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
    private readonly ITenantResolver _tenantResolver;
    private const int RefreshTokenExpirationDays = 7;

    public RefreshTokenCommandHandler(
        IRefreshTokenService refreshTokenService,
        IJwtTokenProvider jwtTokenProvider,
        ITenantResolver tenantResolver)
    {
        _refreshTokenService = refreshTokenService;
        _jwtTokenProvider = jwtTokenProvider;
        _tenantResolver = tenantResolver;
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

        // Check for token reuse (breach detection) - if token was already revoked, revoke entire family
        if (storedToken.IsRevoked)
        {
            await _refreshTokenService.RevokeFamilyAsync(storedToken.FamilyId, cancellationToken);
            return Result<LoginResponse>.Failure(AuthenticationErrors.InvalidRefreshToken);
        }

        // Revoke the current token
        await _refreshTokenService.RevokeAsync(
            request.RefreshToken,
            cancellationToken);

        var accessToken = _jwtTokenProvider.GenerateToken(
            storedToken.UserId,
            storedToken.Email);

        var organizationId = await _tenantResolver.ResolveAsync(storedToken.UserId, cancellationToken) ?? storedToken.OrganizationId;

        // Create next token in the same family
        var newRefreshToken = await _refreshTokenService.CreateNextInFamilyAsync(
            storedToken.UserId,
            storedToken.Email,
            storedToken.FamilyId,
            organizationId,
            RefreshTokenExpirationDays,
            cancellationToken);

        return Result<LoginResponse>.Success(
            new LoginResponse(
                storedToken.UserId,
                accessToken,
                newRefreshToken.Token));
    }
}
