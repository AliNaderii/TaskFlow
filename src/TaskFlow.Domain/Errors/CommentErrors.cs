using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Errors;

public static class CommentErrors
{
    public static readonly Error ContentRequired =
        new(
            "Comment.Content.Required",
            "Comment Content is Required.");

    public static readonly Error ContentTooShort =
        new(
            "Comment.Content.TooShort",
            "Comment content is too short.");

    public static readonly Error ContentTooLong =
        new(
            "Comment.Content.TooLong",
            "Comment content is too long");

    public static readonly Error AlreadyArchived =
        new(
            "Comment.AlreadyArchived", 
            "Comment is already archived.");

    public static readonly Error NotArchived =
        new(
            "Comment.NotArchived", 
            "Comment is not archived.");
}