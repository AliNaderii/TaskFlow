using TaskFlow.Domain.Common;
using TaskFlow.Domain.Constants;

namespace TaskFlow.Domain.Errors;

public static class ProjectErrors
{
    public static readonly Error NameRequired =
        new(
            "project.Name.Required",
            "Project name is required.");

    public static readonly Error NameTooShort =
        new(
            "Project.Name.TooShort",
            $"Project name must be at least {ProjectConstants.NameMinLength} characters.");

    public static readonly Error NameTooLong =
        new(
            "Project.Name.TooLong",
            $"Project name cannot exceed {ProjectConstants.NameMaxLength} characters.");

    public static readonly Error DescriptionTooLong =
        new(
            "Project.Description.TooLong",
            $"Project description cannot exceed {ProjectConstants.DescriptionMaxLength} characters.");

    public static readonly Error AlreadyArchived =
        new(
            "Project.AlreadyArchived",
            "Project is already archived.");

    public static readonly Error AlreadyActive =
        new(
            "Project.AlreadyActive",
            "Project is already active.");
}