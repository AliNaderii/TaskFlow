namespace TaskFlow.Infrastructure.Authentication;

using TaskFlow.Domain.Common;

public sealed class RefreshToken : ITenantEntity
{
    private RefreshToken()
    {
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public Guid OrganizationId { get; private set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsActive => !IsExpired && !IsRevoked;

    public static RefreshToken Create(
        Guid userId,
        string email,
        string token,
        int expirationDays,
        Guid organizationId)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Email = email,
            Token = token,
            OrganizationId = organizationId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays)
        };
    }

    public void Revoke()
    {
        RevokedAt = DateTime.UtcNow;
    }
}