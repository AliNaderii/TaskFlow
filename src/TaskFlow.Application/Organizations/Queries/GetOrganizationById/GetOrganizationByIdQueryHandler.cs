using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Errors;

namespace TaskFlow.Application.Organizations.Queries.GetOrganizationById;

public sealed class GetOrganizationByIdQueryHandler
    : IQueryHandler<GetOrganizationByIdQuery, OrganizationDto>
{
    private readonly IOrganizationRepository _organizationRepository;

    public GetOrganizationByIdQueryHandler(IOrganizationRepository organizationRepository)
    {
        _organizationRepository = organizationRepository;
    }

    public async Task<Result<OrganizationDto>> Handle(
        GetOrganizationByIdQuery request, 
        CancellationToken cancellationToken)
    {
        var organization = await _organizationRepository.GetByIdAsync(
            request.Id, 
            cancellationToken);

        if (organization is null)
            return Result<OrganizationDto>.Failure(OrganizationErrors.NotFound);
        
        var dto = new OrganizationDto(
            organization.Id, 
            organization.Name.Value,
            organization.CreatedAt,
            organization.UpdatedAt,
            organization.ArchivedAt);

        return Result<OrganizationDto>.Success(dto);
    }
}