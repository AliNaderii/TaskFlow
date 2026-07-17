using TaskFlow.Domain.Common;
using TaskFlow.Domain.Constants;

namespace TaskFlow.Domain.Errors;

public static class UserErrors
{
    public static readonly Error NotFound =
        new(
            "User.NotFound",
            "User does not exist.");

    public static readonly Error DisplayNameRequired =
        new(
            "User.Name.Required",
            "Display name is required");

    public static readonly Error DispalyNameIsTooLong =
        new(
            "User.Name.TooLong",
            $"Display name cannot exceed {DisplayNameConstants.MaxLength} characters");
}