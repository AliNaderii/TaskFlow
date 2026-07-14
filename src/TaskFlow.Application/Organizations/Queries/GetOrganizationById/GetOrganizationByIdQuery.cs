using TaskFlow.Application.Abstractions.Messaging;

namespace TaskFlow.Application.Organizations.Queries.GetOrganizationById;

public sealed record GetOrganizationByIdQuery(Guid Id) : IQuery<OrganizationDto>;