using MediatR;
using TaskFlow.Domain.Events;

namespace TaskFlow.Application.Events.Handlers;

internal sealed class TaskAssignedEventHandler : INotificationHandler<TaskAssignedEvent>
{
    public Task Handle(TaskAssignedEvent notification, CancellationToken cancellationToken)
    {
        // TODO: Handle task assigned event (e.g., send notification)
        return Task.CompletedTask;
    }
}