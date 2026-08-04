using TaskFlow.Application.Abstractions.Authentication;
using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.MultiTenancy;
using TaskFlow.Application.Abstractions.Services;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Errors;

namespace TaskFlow.Application.Authentication.Login;

public sealed class LoginCommandHandler
    : ICommandHandler<LoginCommand, LoginResponse>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtTokenProvider _jwtTokenProvider;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ITenantResolver _tenantResolver;

    public LoginCommandHandler(
        IIdentityService identityService,
        IJwtTokenProvider jwtTokenProvider,
        IRefreshTokenService refreshTokenService,
        ITenantResolver tenantResolver)
    {
        _identityService = identityService;
        _jwtTokenProvider = jwtTokenProvider;
        _refreshTokenService = refreshTokenService;
        _tenantResolver = tenantResolver;
    }

    public async Task<Result<LoginResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var userId = await _identityService.ValidateCredentialsAsync(
            request.Email,
            request.Password,
            cancellationToken);

        if (userId is null)
        {
            return Result<LoginResponse>.Failure(AuthenticationErrors.InvalidCredentials);
        }

        var token = _jwtTokenProvider.GenerateToken(
            userId.Value,
            request.Email);

        var organizationId = await _tenantResolver.ResolveAsync(userId.Value, cancellationToken) ?? Guid.Empty;

        var refreshTokenResult = await _refreshTokenService.CreateAsync(
            userId.Value,
            request.Email,
            organizationId,
            cancellationToken);

        return Result<LoginResponse>.Success(
            new LoginResponse(
                userId.Value,
                token,
                refreshTokenResult.Token));
    }
}
