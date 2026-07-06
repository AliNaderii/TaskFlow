using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Errors;

public static class UserErrors
{
    public static readonly Error DisplayNameIsEmpty =
        new(
            "user.name.empty",
            "Display name is required");

    public static readonly Error DispalyNameIsTooLong =
        new(
            "user.name.too_long",
            "Display name cannot exceed 50 characters");
}