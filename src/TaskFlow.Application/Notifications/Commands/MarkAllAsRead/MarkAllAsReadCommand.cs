using MediatR;
using TaskFlow.Application.Abstractions.Messaging;

namespace TaskFlow.Application.Notifications.Commands.MarkAllAsRead;

public sealed record MarkAllAsReadCommand : ICommand<Unit>;