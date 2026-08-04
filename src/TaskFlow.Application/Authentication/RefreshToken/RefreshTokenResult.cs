namespace TaskFlow.Application.Authentication.RefreshToken;

public sealed record RefreshTokenResult(
    Guid UserId,
    string Email,
    string Token,
    DateTime ExpiresAt,
    Guid OrganizationId,
    Guid FamilyId,
    bool IsRevoked);
