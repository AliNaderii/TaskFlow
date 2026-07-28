using MediatR;
using TaskFlow.Domain.Events;

namespace TaskFlow.Application.Events.Handlers;

internal sealed class TaskCompletedEventHandler : INotificationHandler<TaskCompletedEvent>
{
    public Task Handle(TaskCompletedEvent notification, CancellationToken cancellationToken)
    {
        // TODO: Handle task completed event (e.g., send notification)
        return Task.CompletedTask;
    }
}