using TaskFlow.Application.Authentication.RefreshToken;
namespace TaskFlow.Application.Authentication.Login;

public sealed record LoginResponse(
    Guid UserId,
    string Token,
    string? NewRefreshToken);