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

    public async Task<(IReadOnlyList<Project> Items, int TotalCount)> SearchAsync(
        string? keyword,
        bool? isArchived,
        int page,
        int pageSize,
        string? sortBy,
        string? sortDirection,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Projects.AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim().ToLower();
            query = query.Where(p => 
                p.Name.Value.ToLower().Contains(kw) ||
                (p.Description != null && p.Description.Value.ToLower().Contains(kw)));
        }

        if (isArchived.HasValue)
        {
            query = query.Where(p => p.IsArchived == isArchived.Value);
        }

        query = ApplySorting(query, sortBy, sortDirection);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    private static IQueryable<Project> ApplySorting(
        IQueryable<Project> query,
        string? sortBy,
        string? sortDirection)
    {
        var direction = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase)
            ? "desc" : "asc";

        var property = sortBy?.ToLowerInvariant() switch
        {
            "name" => "name",
            "createdat" => "createdat",
            "updatedat" => "updatedat",
            "archivedat" => "archivedat",
            _ => "createdat"
        };

        if (direction == "desc")
        {
            query = property switch
            {
                "name" => query.OrderByDescending(p => p.Name.Value),
                "createdat" => query.OrderByDescending(p => p.CreatedAt),
                "updatedat" => query.OrderByDescending(p => p.UpdatedAt),
                "archivedat" => query.OrderByDescending(p => p.ArchivedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };
        }
        else
        {
            query = property switch
            {
                "name" => query.OrderBy(p => p.Name.Value),
                "createdat" => query.OrderBy(p => p.CreatedAt),
                "updatedat" => query.OrderBy(p => p.UpdatedAt),
                "archivedat" => query.OrderBy(p => p.ArchivedAt),
                _ => query.OrderBy(p => p.CreatedAt)
            };
        }

        return query;
    }
}
