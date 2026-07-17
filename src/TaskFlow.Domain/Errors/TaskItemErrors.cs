using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Errors;

public static class TaskItemErrors
{
    public static readonly Error NotFound =
        new(
            "Project.NotFound",
            "Project does not exist.");

    public static readonly Error TitleRequired =
        new(
            "Task.Title.Required", 
            "Task title is required.");

    public static readonly Error TitleTooShort =
        new(
            "Task.Title.TooShort", 
            "Task title is too short.");

    public static readonly Error TitleTooLong =
        new(
            "Task.Title.TooLong", 
            "Task title is too long.");

    public static readonly Error DescriptionTooLong =
        new(
            "Task.Description.TooLong", 
            "Task description is too long.");

    public static readonly Error AlreadyArchived =
        new(
            "Task.AlreadyArchived", 
            "Task is already archived.");

    public static readonly Error NotArchived =
        new(
            "Task.NotArchived", 
            "Task is not archived.");
    
    public static readonly Error InvalidCreatorUserId =
        new(
            "Task.CreatorUserId",
            "The user id provided for task is invalid");
}