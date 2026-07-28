using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Events;

public sealed record TaskAssignedEvent : IDomainEvent
{
    public Guid TaskItemId { get; init; }
    public Guid AssigneeUserId { get; init; }
    public Guid AssignedByUserId { get; init; }
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid EventId { get; init; } = Guid.NewGuid();

    private TaskAssignedEvent() { }

    public static TaskAssignedEvent Create(Guid taskItemId, Guid assigneeUserId, Guid assignedByUserId)
    {
        return new TaskAssignedEvent
        {
            TaskItemId = taskItemId,
            AssigneeUserId = assigneeUserId,
            AssignedByUserId = assignedByUserId
        };
    }
}
