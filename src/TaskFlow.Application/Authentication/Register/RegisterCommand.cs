using TaskFlow.Application.Abstractions.Messaging;

namespace TaskFlow.Application.Authentication.Register;

public sealed record RegisterCommand(
    string Email,
    string DisplayName,
    string Password)
    : ICommand<Guid>;