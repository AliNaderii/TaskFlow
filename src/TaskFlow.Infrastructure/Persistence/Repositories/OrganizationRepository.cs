using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

public sealed class OrganizationRepository : IOrganizationRepository
{
    private readonly ApplicationDbContext _context;

    public OrganizationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        Organization organization, 
        CancellationToken cancellationToken = default)
    {
        await _context.Organizations.AddAsync(
            organization, 
            cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(
        OrganizationName name, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Organizations.AnyAsync(
            x => x.Name == name, 
            cancellationToken);
    }

    public async Task<Organization?> GetByIdAsync(
        Guid Id, 
        CancellationToken cancellationToken)
    {
        return await _context.Organizations.FirstOrDefaultAsync(
            x => x.Id == Id,
            cancellationToken);
    }
}