using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Domain.Common;

namespace TaskFlow.Infrastructure.Messaging;

internal sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync(IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handler = _serviceProvider.GetService(handlerType);

            if (handler is null)
            {
                continue;
            }

            var handleMethod = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.Handle));
            if (handleMethod is null)
            {
                continue;
            }

            await (Task)handleMethod.Invoke(handler, [domainEvent, cancellationToken])!;
        }
    }
}
