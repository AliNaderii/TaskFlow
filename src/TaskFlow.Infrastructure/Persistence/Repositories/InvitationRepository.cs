using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

public sealed class InvitationRepository : IInvitationRepository
{
    private readonly ApplicationDbContext _context;

    public InvitationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Invitation?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Invitations
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Invitation?> GetByTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        return await _context.Invitations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Token.Value == token, cancellationToken);
    }

    public async Task<bool> ExistsPendingByEmailAsync(
        string email,
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Invitations
            .AnyAsync(x => x.Email == email.ToLowerInvariant()
                && x.OrganizationId == organizationId
                && x.Status == TaskFlow.Domain.Enums.InvitationStatus.Pending,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Invitation>> GetByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Invitations
            .Where(x => x.OrganizationId == organizationId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Invitation invitation,
        CancellationToken cancellationToken = default)
    {
        await _context.Invitations.AddAsync(invitation, cancellationToken);
    }

    public void Update(Invitation invitation)
    {
        _context.Invitations.Update(invitation);
    }
}