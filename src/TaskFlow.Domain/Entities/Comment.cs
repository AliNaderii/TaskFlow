using TaskFlow.Domain.Common;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Domain.Entities;

public sealed class Comment : AuditableEntity, ITenantEntity
{
    public Guid OrganizationId {get; private set;}
    public Guid TaskId { get; private set; }
    public Guid AuthorUserId { get; private set; }
    public CommentContent Content { get; private set; } = null!;
    public bool IsEdited { get; private set; }
    public bool IsArchived { get; private set; }
    public TaskItem Task { get; private set; } = null!;
    public User Author { get; private set; } = null!;


    private Comment() {}

    private Comment(
            Guid organizationId,
            Guid taskId,
            Guid authorUserId,
            CommentContent content)
    {
        OrganizationId = organizationId;
        TaskId = taskId;
        AuthorUserId = authorUserId;
        Content = content;
        IsEdited = false;
        IsArchived = false;
    }

    public static Result<Comment> Create(
        Guid organizationId,
        Guid taskId,
        Guid authorUserId,
        CommentContent content)
    {
        if (organizationId == Guid.Empty)
        {
            return Result<Comment>.Failure(CommentErrors.InvalidOrganizationId);
        }

        var comment = new Comment(
            organizationId,
            taskId,
            authorUserId,
            content);

        return Result<Comment>.Success(comment);
    }

    public BaseResult UpdateContent(string content)
    {
        var result = CommentContent.Create(content);

        if (result.IsFailure)
        {
            return BaseResult.Failure(result.Error);
        }
        
        Content = result.Value;
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