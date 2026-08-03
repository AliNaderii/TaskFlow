using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Events;

namespace TaskFlow.Application.Events.Handlers;

public sealed class MembershipAddedEventHandler : IDomainEventHandler<MembershipAddedEvent>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MembershipAddedEventHandler(
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(MembershipAddedEvent notification, CancellationToken cancellationToken)
    {
        var notificationResult = Notification.Create(
            notification.OrganizationId,
            notification.UserId,
            NotificationType.MembershipAdded,
            "Welcome to the Organization",
            $"You have been added to the organization with role: {notification.Role}",
            null);

        if (notificationResult.IsSuccess)
        {
            await _notificationRepository.AddAsync(notificationResult.Value, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
