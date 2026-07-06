using TaskFlow.Domain.Common;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Domain.Entities;

public sealed class Comment : AuditableEntity
{
    public Guid TaskId { get; private set; }

    public Guid AuthorUserId { get; private set; }

    public CommentContent Content { get; private set; }

    public bool IsEdited { get; private set; }

    public bool IsArchived { get; private set; }

    private Comment() { }

    private Comment(
            Guid taskId,
            Guid authorUserId,
            CommentContent content)
    {
        TaskId = taskId;
        AuthorUserId = authorUserId;
        Content = content;
        IsEdited = false;
        IsArchived = false;
    }

    public static Result<Comment> Create(
        Guid taskId,
        Guid authorUserId,
        CommentContent content)
    {
        var comment = new Comment(
            taskId,
            authorUserId,
            content);

        return Result<Comment>.Success(comment);
    }

    public BaseResult Edit(CommentContent content)
    {
        Content = content;
        IsEdited = true;

        return BaseResult.Success();
    }

    public BaseResult Archive()
    {
        if (IsArchived)
        {
            return BaseResult.Failure(CommentErrors.AlreadyArchived);
        }

        IsArchived = true;

        return BaseResult.Success();
    }

    public BaseResult Restore()
    {
        if (!IsArchived)
        {
            return BaseResult.Failure(CommentErrors.NotArchived);
        }

        IsArchived = false;

        return BaseResult.Success();
    }
}