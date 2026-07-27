using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

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

    public async Task<Guid?> GetOrganizationIdForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Memberships
            .Where(x => x.UserId == userId)
            .Select(x => (Guid?)x.OrganizationId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> IsUserAdminAsync(
        Guid userId,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Memberships.AnyAsync(
            x => x.UserId == userId
                && x.OrganizationId == organizationId
                && (x.Role == MembershipRole.Admin || x.Role == MembershipRole.Owner)
                && x.Status == MembershipStatus.Active,
            cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(
        string email,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Memberships
            .Join(_context.Users,
                m => m.UserId,
                u => u.Id,
                (m, u) => new { m, u })
            .AnyAsync(x => x.u.Email == email
                && x.m.OrganizationId == organizationId
                && x.m.Status == MembershipStatus.Active,
            cancellationToken);
    }

    public async Task<Membership?> GetByUserIdAsync(
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
