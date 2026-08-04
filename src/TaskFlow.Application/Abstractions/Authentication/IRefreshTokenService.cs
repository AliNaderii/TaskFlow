using TaskFlow.Application.Authentication.RefreshToken;

namespace TaskFlow.Application.Abstractions.Authentication;

public interface IRefreshTokenService
{
    Task<RefreshTokenResult> CreateAsync(
        Guid userId,
        string email,
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task<RefreshTokenResult> CreateNextInFamilyAsync(
        Guid userId,
        string email,
        Guid familyId,
        Guid organizationId,
        int expirationDays,
        CancellationToken cancellationToken = default);


    Task<RefreshTokenResult?> GetAsync(
        string token,
        CancellationToken cancellationToken = default);


    Task<bool> RevokeAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task<bool> RevokeFamilyAsync(
        Guid familyId,
        CancellationToken cancellationToken = default);
}
