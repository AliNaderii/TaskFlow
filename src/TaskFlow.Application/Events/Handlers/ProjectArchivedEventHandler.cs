using MediatR;
using TaskFlow.Domain.Events;

namespace TaskFlow.Application.Events.Handlers;

internal sealed class ProjectArchivedEventHandler : INotificationHandler<ProjectArchivedEvent>
{
    public Task Handle(ProjectArchivedEvent notification, CancellationToken cancellationToken)
    {
        // TODO: Handle project archived event (e.g., send notification)
        return Task.CompletedTask;
    }
}