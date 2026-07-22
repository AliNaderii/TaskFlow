using TaskFlow.Application.Authentication.RefreshToken;

namespace TaskFlow.Application.Abstractions.Authentication;

public interface IRefreshTokenService
{
    Task<RefreshTokenResult> CreateAsync(
        Guid userId,
        string email,
        CancellationToken cancellationToken = default);


    Task<RefreshTokenResult?> GetAsync(
        string token,
        CancellationToken cancellationToken = default);


    Task<bool> RevokeAsync(
        string token,
        CancellationToken cancellationToken = default);
}