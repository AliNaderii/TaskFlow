namespace TaskFlow.Application.Abstractions.Services;

public interface IIdentityService
{
    Task<bool> CreateUserAsync(
        Guid domainUserId,
        string email,
        string password,
        CancellationToken cancellationToken = default);


    Task<Guid?> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);
}