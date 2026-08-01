using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

internal sealed class CommentRepository : ICommentRepository
{
    private readonly ApplicationDbContext _context;

    public CommentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Comment?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Comments
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Comment>> GetByTaskIdAsync(
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Comments
            .Where(x => x.TaskId == taskId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Comment comment,
        CancellationToken cancellationToken = default)
    {
        await _context.Comments.AddAsync(
            comment,
            cancellationToken);
    }

    public async Task<(IReadOnlyList<Comment> Items, int TotalCount)> SearchAsync(
        Guid? taskId,
        string? keyword,
        Guid? authorUserId,
        DateTime? createdAtFrom,
        DateTime? createdAtTo,
        int page,
        int pageSize,
        string? sortBy,
        string? sortDirection,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Comments.AsQueryable();

        if (taskId.HasValue)
        {
            query = query.Where(c => c.TaskId == taskId.Value);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim().ToLower();
            query = query.Where(c => c.Content.Value.ToLower().Contains(kw));
        }

        if (authorUserId.HasValue)
        {
            query = query.Where(c => c.AuthorUserId == authorUserId.Value);
        }

        if (createdAtFrom.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= createdAtFrom.Value);
        }

        if (createdAtTo.HasValue)
        {
            query = query.Where(c => c.CreatedAt <= createdAtTo.Value);
        }

        query = ApplySorting(query, sortBy, sortDirection);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    private static IQueryable<Comment> ApplySorting(
        IQueryable<Comment> query,
        string? sortBy,
        string? sortDirection)
    {
        var direction = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase)
            ? "desc" : "asc";

        var property = sortBy?.ToLowerInvariant() switch
        {
            "content" => "content",
            "createdat" => "createdat",
            "updatedat" => "updatedat",
            _ => "createdat"
        };

        if (direction == "desc")
        {
            query = property switch
            {
                "content" => query.OrderByDescending(c => c.Content.Value),
                "createdat" => query.OrderByDescending(c => c.CreatedAt),
                "updatedat" => query.OrderByDescending(c => c.UpdatedAt),
                _ => query.OrderByDescending(c => c.CreatedAt)
            };
        }
        else
        {
            query = property switch
            {
                "content" => query.OrderBy(c => c.Content.Value),
                "createdat" => query.OrderBy(c => c.CreatedAt),
                "updatedat" => query.OrderBy(c => c.UpdatedAt),
                _ => query.OrderBy(c => c.CreatedAt)
            };
        }

        return query;
    }
}
