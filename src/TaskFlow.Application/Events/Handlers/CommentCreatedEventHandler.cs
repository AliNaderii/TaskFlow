using MediatR;
using TaskFlow.Domain.Events;

namespace TaskFlow.Application.Events.Handlers;

internal sealed class CommentCreatedEventHandler : INotificationHandler<CommentCreatedEvent>
{
    public Task Handle(CommentCreatedEvent notification, CancellationToken cancellationToken)
    {
        // TODO: Handle comment created event (e.g., send notification)
        return Task.CompletedTask;
    }
}