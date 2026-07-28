using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Application.Abstractions.Authentication;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Notifications.Queries.GetUnreadCount;

internal sealed class GetUnreadCountQueryHandler
    : IQueryHandler<GetUnreadCountQuery, int>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUser _currentUser;

    public GetUnreadCountQueryHandler(
        INotificationRepository notificationRepository,
        ICurrentUser currentUser)
    {
        _notificationRepository = notificationRepository;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(
        GetUnreadCountQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.Id.HasValue)
        {
            return Result<int>.Failure(
                new Error("auth.user_not_found", "User not authenticated."));
        }

        var count = await _notificationRepository.GetUnreadCountAsync(
            _currentUser.Id.Value,
            cancellationToken);

        return Result<int>.Success(count);
    }
}