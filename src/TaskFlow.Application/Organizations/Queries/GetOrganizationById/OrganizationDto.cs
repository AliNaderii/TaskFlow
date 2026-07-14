namespace TaskFlow.Application.Organizations.Queries.GetOrganizationById;

public sealed record OrganizationDto(
    Guid Id, 
    string Name,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? ArchivedAt);