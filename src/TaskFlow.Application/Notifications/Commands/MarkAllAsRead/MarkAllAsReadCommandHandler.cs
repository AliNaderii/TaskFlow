using MediatR;
using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Application.Abstractions.Authentication;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Errors;

namespace TaskFlow.Application.Notifications.Commands.MarkAllAsRead;

internal sealed class MarkAllAsReadCommandHandler
    : ICommandHandler<MarkAllAsReadCommand, Unit>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public MarkAllAsReadCommandHandler(
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<Unit>> Handle(
        MarkAllAsReadCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.Id.HasValue)
        {
            return Result<Unit>.Failure(
                new Error("auth.user_not_found", "User not authenticated."));
        }

        var notifications = await _notificationRepository.GetUnreadByUserIdAsync(
            _currentUser.Id.Value,
            cancellationToken);

        foreach (var notification in notifications)
        {
            notification.MarkAsRead();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
