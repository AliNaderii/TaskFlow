using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Events;

public sealed record ProjectArchivedEvent : IDomainEvent
{
    public Guid ProjectId { get; init; }
    public Guid ArchivedByUserId { get; init; }
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid EventId { get; init; } = Guid.NewGuid();

    private ProjectArchivedEvent() { }

    public static ProjectArchivedEvent Create(Guid projectId, Guid archivedByUserId)
    {
        return new ProjectArchivedEvent
        {
            ProjectId = projectId,
            ArchivedByUserId = archivedByUserId
        };
    }
}