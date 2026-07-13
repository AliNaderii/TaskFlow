using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Errors;

public static class EmailErrors
{
    public static readonly Error Required =
        new(
            "Email.Required",
            "Email is required.");

    public static readonly Error Invalid =
        new(
            "Email.Invalid",
            "Email format is invalid.");
}