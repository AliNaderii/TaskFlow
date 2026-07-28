using MediatR;
using TaskFlow.Application.Abstractions.Messaging;

namespace TaskFlow.Application.Notifications.Commands.MarkAsRead;

public sealed record MarkAsReadCommand(Guid NotificationId) : ICommand<Unit>;