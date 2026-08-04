using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Abstractions.Messaging;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(
        IReadOnlyCollection<IDomainEvent> domainEvents, 
        CancellationToken cancellationToken = default);
}
