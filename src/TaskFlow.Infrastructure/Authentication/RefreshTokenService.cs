using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Application.Abstractions.Authentication;
using TaskFlow.Application.Authentication.RefreshToken;

namespace TaskFlow.Infrastructure.Authentication;

public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly ApplicationDbContext _context;
    private readonly JwtOptions _options;


    public RefreshTokenService(
        ApplicationDbContext context,
        IOptions<JwtOptions> options)
    {
        _context = context;
        _options = options.Value;
    }


    public async Task<RefreshTokenResult> CreateAsync(
        Guid userId,
        string email,
        CancellationToken cancellationToken = default)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        var refreshToken = RefreshToken.Create(
            userId,
            email,
            token,
            _options.RefreshTokenExpirationDays);

        _context.RefreshTokens.Add(refreshToken);

        await _context.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResult(
            refreshToken.UserId,
            refreshToken.Email,
            refreshToken.Token,
            refreshToken.ExpiresAt);
    }


    public async Task<RefreshTokenResult?> GetAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(
                x => x.Token == token,
                cancellationToken);
        
        if (refreshToken is null)
        {
            return null;
        }

         if (!refreshToken.IsActive)
        {
            return null;
        }

        return new RefreshTokenResult(
            refreshToken.UserId,
            refreshToken.Email,
            refreshToken.Token,
            refreshToken.ExpiresAt);
    }

    public async Task<bool> RevokeAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(
                x => x.Token == token,
                cancellationToken);

        if (refreshToken is null)
        {
            return false;
        }

        if (!refreshToken.IsActive)
        {
            return false;
        }

        refreshToken.Revoke();

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}