using Microsoft.AspNetCore.Identity;
using TaskFlow.Application.Abstractions.Services;

namespace TaskFlow.Infrastructure.Identity;

public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;


    public IdentityService(
        UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }


    public async Task<bool> CreateUserAsync(
        Guid domainUserId,
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            DomainUserId = domainUserId,
            Email = email,
            UserName = email
        };


        var result =
            await _userManager.CreateAsync(
                user,
                password);

        return result.Succeeded;
    }

    public async Task<Guid?> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return null;
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, password);

        if (!passwordValid)
        {
            return null;
        }

        return user.DomainUserId;
    }
}