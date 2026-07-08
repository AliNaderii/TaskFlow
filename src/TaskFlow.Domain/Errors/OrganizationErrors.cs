using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Errors;

public static class OrganizationErrors
{
    public static readonly Error NameRequired =
        new(
            "Organization.Name.Empty",
            "Organization name is required.");

    public static readonly Error NameTooLong =
        new(
            "Organization.Name.TooLong",
            "Organization name cannot exceed 100 characters.");
    
    public static readonly Error NameTooShort =
        new(
            "Organization.Name.TooShort", 
            "Organization name is too short.");
    public static readonly Error NameAlreadyExists =
        new(
            "Organization.Name.NameAlreadyExists", 
            "Organization name already exists.");
}