namespace TaskFlow.Api.Contracts.Authentication;

public sealed record ApiRegisterRequest(
    string Email,
    string DisplayName,
    string Password);