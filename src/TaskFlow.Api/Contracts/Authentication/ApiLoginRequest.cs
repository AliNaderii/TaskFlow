namespace TaskFlow.Api.Contracts.Authentication;

public sealed record ApiLoginRequest(
    string Email,
    string Password);