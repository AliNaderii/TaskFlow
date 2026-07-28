using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Events;

public sealed record CommentCreatedEvent : IDomainEvent
{
    public Guid CommentId { get; init; }
    public Guid TaskItemId { get; init; }
    public Guid AuthorUserId { get; init; }
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid EventId { get; init; } = Guid.NewGuid();

    private CommentCreatedEvent() { }

    public static CommentCreatedEvent Create(Guid commentId, Guid taskItemId, Guid authorUserId)
    {
        return new CommentCreatedEvent
        {
            CommentId = commentId,
            TaskItemId = taskItemId,
            AuthorUserId = authorUserId
        };
    }
}