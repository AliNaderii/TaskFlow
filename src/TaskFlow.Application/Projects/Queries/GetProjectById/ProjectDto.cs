namespace TaskFlow.Application.Projects.Queries.GetProjectById;

public sealed record ProjectDto(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string? Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? ArchivedAt);