using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Abstractions.Persistence;

public interface ICommentRepository
{
    Task<Comment?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Comment>> GetByTaskIdAsync(
        Guid taskId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Comment comment,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Comment> Items, int TotalCount)> SearchAsync(
        Guid? taskId,
        string? keyword,
        Guid? authorUserId,
        DateTime? createdAtFrom,
        DateTime? createdAtTo,
        int page,
        int pageSize,
        string? sortBy,
        string? sortDirection,
        CancellationToken cancellationToken = default);
}
