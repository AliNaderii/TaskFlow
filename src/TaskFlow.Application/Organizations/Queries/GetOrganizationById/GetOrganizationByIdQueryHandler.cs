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
        var result = await _organizationRepository.GetByIdAsync(
            request.Id, 
            cancellationToken);

        if (result is null)
            return Result<OrganizationDto>.Failure(OrganizationErrors.NotFound);
        
        var dto = new OrganizationDto(
            result.Id, 
            result.Name.Value,
            result.CreatedAt,
            result.UpdatedAt,
            result.ArchivedAt);

        return Result<OrganizationDto>.Success(dto);
    }
}