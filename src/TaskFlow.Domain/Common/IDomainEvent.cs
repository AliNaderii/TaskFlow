using MediatR;

namespace TaskFlow.Domain.Common;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
    Guid EventId { get; }
}
