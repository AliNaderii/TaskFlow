using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Events;

namespace TaskFlow.Application.Events.Handlers;

public sealed class ProjectArchivedEventHandler : IDomainEventHandler<ProjectArchivedEvent>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProjectRepository _projectRepository;
    private readonly IMembershipRepository _membershipRepository;

    public ProjectArchivedEventHandler(
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork,
        IProjectRepository projectRepository,
        IMembershipRepository membershipRepository)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _projectRepository = projectRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task Handle(ProjectArchivedEvent notification, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(notification.ProjectId, cancellationToken);
        if (project is null)
        {
            return;
        }

        var memberships = await _membershipRepository.GetByOrganizationIdAsync(project.OrganizationId, cancellationToken);
        var memberUserIds = memberships
            .Where(m => m.UserId != notification.ArchivedByUserId)
            .Select(m => m.UserId)
            .Distinct()
            .ToList();

        foreach (var memberUserId in memberUserIds)
        {
            var notificationResult = Notification.Create(
                project.OrganizationId,
                memberUserId,
                NotificationType.ProjectArchived,
                "Project Archived",
                $"Project '{project.Name.Value}' has been archived",
                notification.ProjectId);

            if (notificationResult.IsSuccess)
            {
                await _notificationRepository.AddAsync(notificationResult.Value, cancellationToken);
            }
        }

        if (memberUserIds.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
