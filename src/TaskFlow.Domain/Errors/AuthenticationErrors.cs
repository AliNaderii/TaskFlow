using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Errors;

public static class AuthenticationErrors
{
    public static readonly Error InvalidCredentials =
        new(
            "Authentication.InvalidCredentials",
            "Invalid email or password.");
}