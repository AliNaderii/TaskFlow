using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Errors;

public static class EmailErrors
{
    public static readonly Error Empty =
        new(
            "email.empty",
            "Email is required.");

    public static readonly Error Invalid =
        new(
            "email.invalid",
            "Email format is invalid.");
}