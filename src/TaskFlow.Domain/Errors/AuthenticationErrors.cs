using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Errors;

public static class AuthenticationErrors
{
    public static readonly Error InvalidCredentials =
        new(
            "Authentication.InvalidCredentials",
            "Invalid email or password.");
    
    public static readonly Error ExpiredRefreshToken =
        new(
            "Authentication.ExpiredRefreshToken",
            "Refresh token has expired.");

    public static readonly Error InvalidRefreshToken =
        new(
            "Authentication.InvalidRefreshToken",
            "Refresh token is invalid.");
}