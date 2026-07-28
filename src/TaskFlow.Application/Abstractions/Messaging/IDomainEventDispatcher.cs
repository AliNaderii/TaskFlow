using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Abstractions.Messaging;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents, 
        CancellationToken cancellationToken = default);
}