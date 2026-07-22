using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Authentication.Login;

namespace TaskFlow.Application.Authentication.RefreshToken;

public sealed record RefreshTokenCommand(
    string RefreshToken)
    : ICommand<LoginResponse>;