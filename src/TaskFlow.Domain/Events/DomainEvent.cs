using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Events;

public abstract record DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid EventId { get; init; } = Guid.NewGuid();

    protected DomainEvent() { }
}