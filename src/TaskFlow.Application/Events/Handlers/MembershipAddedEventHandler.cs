using MediatR;
using TaskFlow.Domain.Events;

namespace TaskFlow.Application.Events.Handlers;

internal sealed class MembershipAddedEventHandler : INotificationHandler<MembershipAddedEvent>
{
    public Task Handle(MembershipAddedEvent notification, CancellationToken cancellationToken)
    {
        // TODO: Handle membership added event (e.g., send invitation email)
        return Task.CompletedTask;
    }
}