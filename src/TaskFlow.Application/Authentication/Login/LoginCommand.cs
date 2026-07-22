using TaskFlow.Application.Abstractions.Messaging;

namespace TaskFlow.Application.Authentication.Login;

public sealed record LoginCommand(
    string Email,
    string Password)
    : ICommand<LoginResponse>;