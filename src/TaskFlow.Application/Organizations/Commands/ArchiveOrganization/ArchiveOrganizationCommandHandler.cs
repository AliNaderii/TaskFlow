using MediatR;
using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Errors;

namespace TaskFlow.Application.Organizations.Commands.ArchiveOrganization;

public sealed class ArchiveOrganizationCommandHandler
    : ICommandHandler<ArchiveOrganizationCommand>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ArchiveOrganizationCommandHandler(
        IOrganizationRepository organizationRepository,
        IUnitOfWork unitOfWork)
    {
        _organizationRepository = organizationRepository;
        _unitOfWork = unitOfWork;
    }
    public async Task<BaseResult> Handle(
        ArchiveOrganizationCommand request, 
        CancellationToken cancellationToken)
    {
        var organization = await _organizationRepository.GetByIdAsync(
            request.Id,
            cancellationToken);
        
        if (organization is null)
        {
            return BaseResult.Failure(OrganizationErrors.NotFound);
        }

        var archiveResult = organization.Archive();

        if (archiveResult.IsFailure)
        {
            return archiveResult;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BaseResult.Success();
    }
}