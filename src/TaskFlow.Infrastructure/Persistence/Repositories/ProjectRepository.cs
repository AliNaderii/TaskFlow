using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

internal sealed class ProjectRepository : IProjectRepository
{
    private readonly ApplicationDbContext _context;

    public ProjectRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Project?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .AnyAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task AddAsync(
        Project project,
        CancellationToken cancellationToken = default)
    {
        await _context.Projects.AddAsync(
            project,
            cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(
        ProjectName name,
        Guid? excludedProjectId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _context.Projects.AnyAsync(
            project => 
                project.Name == name
                && (!excludedProjectId.HasValue || project.Id != excludedProjectId),
            cancellationToken);
        
        return result;
    }
}