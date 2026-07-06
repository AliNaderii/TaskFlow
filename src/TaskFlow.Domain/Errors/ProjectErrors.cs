using TaskFlow.Domain.Common;
using TaskFlow.Domain.Constants;

namespace TaskFlow.Domain.Errors;

public static class ProjectErrors
{
    public static readonly Error NameIsEmpty =
        new(
            "project.name.empty",
            "Project name is required.");

    public static readonly Error NameTooShort =
        new(
            "project.name.too_short",
            $"Project name must be at least {ProjectConstants.NameMinLength} characters.");

    public static readonly Error NameTooLong =
        new(
            "project.name.too_long",
            $"Project name cannot exceed {ProjectConstants.NameMaxLength} characters.");

    public static readonly Error DescriptionTooLong =
        new(
            "project.description.too_long",
            $"Project description cannot exceed {ProjectConstants.DescriptionMaxLength} characters.");

    public static readonly Error AlreadyArchived =
        new(
            "project.already_archived",
            "Project is already archived.");

    public static readonly Error AlreadyActive =
        new(
            "project.already_active",
            "Project is already active.");
}