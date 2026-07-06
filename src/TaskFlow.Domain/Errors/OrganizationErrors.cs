using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Errors;

public static class OrganizationErrors
{
    public static readonly Error NameIsEmpty =
        new(
            "organization.name.empty",
            "Organization name is required.");

    public static readonly Error NameTooLong =
        new(
            "organization.name.too_long",
            "Organization name cannot exceed 100 characters.");
}