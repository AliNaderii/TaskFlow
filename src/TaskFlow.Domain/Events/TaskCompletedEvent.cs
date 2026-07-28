using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Events;

public sealed record TaskCompletedEvent : IDomainEvent
{
    public Guid TaskItemId { get; init; }
    public Guid CompletedByUserId { get; init; }
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid EventId { get; init; } = Guid.NewGuid();

    private TaskCompletedEvent() { }

    public static TaskCompletedEvent Create(Guid taskItemId, Guid completedByUserId)
    {
        return new TaskCompletedEvent
        {
            TaskItemId = taskItemId,
            CompletedByUserId = completedByUserId
        };
    }
}