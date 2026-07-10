using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

public sealed class MembershipRepository : IMembershipRepository
{
    private readonly ApplicationDbContext _context;

    public MembershipRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task AddAsync(
        Membership membership, 
        CancellationToken cancellationToken = default)
    {
        await _context.Memberships.AddAsync(
            membership,
            cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid userId, 
        Guid organizationId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Memberships.AnyAsync(
            x => x.UserId == userId 
            && x.OrganizationId == organizationId,
            cancellationToken);
    }

    public async Task<Membership?> GetAsync(
        Guid userId, 
        Guid organizationId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Memberships.FirstOrDefaultAsync(
            x => x.UserId == userId
            && x.OrganizationId == organizationId,
            cancellationToken);
    }
}