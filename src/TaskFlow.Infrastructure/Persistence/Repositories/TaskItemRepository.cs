using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Infrastructure.Persistence.Repositories;

internal sealed class TaskItemRepository : ITaskItemRepository
{
    private readonly ApplicationDbContext _context;

    public TaskItemRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TaskItem?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.TaskItems
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<TaskItem>> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        return await _context.TaskItems
            .Where(x => x.ProjectId == projectId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TaskItem>> GetAssignedToUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.TaskItems
            .Where(x => x.AssigneeUserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        TaskItem taskItem,
        CancellationToken cancellationToken = default)
    {
        await _context.TaskItems.AddAsync(
            taskItem,
            cancellationToken);
    }

    public async Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> SearchAsync(
        Guid? projectId,
        string? keyword,
        TaskItemStatus? status,
        TaskItemPriority? priority,
        Guid? assigneeUserId,
        DateTime? dueDateFrom,
        DateTime? dueDateTo,
        bool? isArchived,
        int page,
        int pageSize,
        string? sortBy,
        string? sortDirection,
        CancellationToken cancellationToken = default)
    {
        var query = _context.TaskItems.AsQueryable();

        if (projectId.HasValue)
        {
            query = query.Where(t => t.ProjectId == projectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim().ToLower();
            query = query.Where(t => 
                t.Title.Value.ToLower().Contains(kw) ||
                (t.Description != null && t.Description.Value.ToLower().Contains(kw)));
        }

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        if (priority.HasValue)
        {
            query = query.Where(t => t.Priority == priority.Value);
        }

        if (assigneeUserId.HasValue)
        {
            query = query.Where(t => t.AssigneeUserId == assigneeUserId.Value);
        }

        if (dueDateFrom.HasValue)
        {
            query = query.Where(t => t.DueDate >= dueDateFrom.Value);
        }

        if (dueDateTo.HasValue)
        {
            query = query.Where(t => t.DueDate <= dueDateTo.Value);
        }

        if (isArchived.HasValue)
        {
            query = query.Where(t => t.IsArchived == isArchived.Value);
        }

        query = ApplySorting(query, sortBy, sortDirection);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    private static IQueryable<TaskItem> ApplySorting(
        IQueryable<TaskItem> query,
        string? sortBy,
        string? sortDirection)
    {
        var direction = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase)
            ? "desc" : "asc";

        var property = sortBy?.ToLowerInvariant() switch
        {
            "title" => "title",
            "status" => "status",
            "priority" => "priority",
            "duedate" => "duedate",
            "createdat" => "createdat",
            "updatedat" => "updatedat",
            _ => "createdat"
        };

        if (direction == "desc")
        {
            query = property switch
            {
                "title" => query.OrderByDescending(t => t.Title.Value),
                "status" => query.OrderByDescending(t => t.Status),
                "priority" => query.OrderByDescending(t => t.Priority),
                "duedate" => query.OrderByDescending(t => t.DueDate),
                "createdat" => query.OrderByDescending(t => t.CreatedAt),
                "updatedat" => query.OrderByDescending(t => t.UpdatedAt),
                _ => query.OrderByDescending(t => t.CreatedAt)
            };
        }
        else
        {
            query = property switch
            {
                "title" => query.OrderBy(t => t.Title.Value),
                "status" => query.OrderBy(t => t.Status),
                "priority" => query.OrderBy(t => t.Priority),
                "duedate" => query.OrderBy(t => t.DueDate),
                "createdat" => query.OrderBy(t => t.CreatedAt),
                "updatedat" => query.OrderBy(t => t.UpdatedAt),
                _ => query.OrderBy(t => t.CreatedAt)
            };
        }

        return query;
    }
}
