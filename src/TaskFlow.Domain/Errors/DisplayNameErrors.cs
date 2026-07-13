using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Errors;

public static class DisplayNameErrors
{
    public static readonly Error Required =
        new(
            "DisplayName.Required",
            "Display name is required.");

    public static readonly Error TooShort =
        new(
            "DisplayName.TooShort",
            "Display name should be at least 3 characters.");

    public static readonly Error TooLong =
        new(
            "DisplayName.TooLong",
            "Display name should not be more than 20 characters.");
}