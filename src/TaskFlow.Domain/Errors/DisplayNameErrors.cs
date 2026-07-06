using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Errors;

public static class DisplayNameErrors
{
    public static readonly Error Empty =
        new(
            "display_name.empty",
            "Display name is required.");

    public static readonly Error TooShort =
        new(
            "dispaly_name.too_short",
            "Display name should be at least 3 characters.");

    public static readonly Error TooLong =
        new(
            "dispaly_name.too_long",
            "Display name should not be more than 20 characters.");
}