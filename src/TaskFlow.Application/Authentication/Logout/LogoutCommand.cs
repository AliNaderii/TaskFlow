using TaskFlow.Application.Abstractions.Messaging;

namespace TaskFlow.Application.Authentication.Logout;

public sealed record LogoutCommand(
    string RefreshToken)
    : ICommand;