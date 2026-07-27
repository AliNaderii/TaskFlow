using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Errors;

public static class AuthorizationErrors
{
    public static readonly Error Forbidden = new("authorization.forbidden", "Access forbidden.");
}