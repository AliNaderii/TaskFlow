namespace TaskFlow.Application.Authentication.RefreshToken;

public sealed record RefreshTokenResult(
    Guid UserId,
    string Email,
    string Token,
    DateTime ExpiresAt);