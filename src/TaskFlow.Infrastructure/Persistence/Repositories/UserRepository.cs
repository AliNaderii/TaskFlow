using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        User user, 
        CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(
            user,
            cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(
        Email email, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Users.AnyAsync(
            user => user.Email == email,
            cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(
        Email email, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Users.FirstOrDefaultAsync(
            user => user.Email == email,
            cancellationToken);
    }
}