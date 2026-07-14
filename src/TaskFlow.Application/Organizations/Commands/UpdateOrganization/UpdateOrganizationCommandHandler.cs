using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Domain.Common;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Organizations.Commands.UpdateOrganization;

public sealed class UpdateOrganizationCommandHandler :
    ICommandHandler<UpdateOrganizationCommand>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateOrganizationCommandHandler(
        IOrganizationRepository organizationRepository,
        IUnitOfWork unitOfWork)
    {
        _organizationRepository = organizationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseResult> Handle(
        UpdateOrganizationCommand request, 
        CancellationToken cancellationToken)
    {
        var organization = await _organizationRepository.GetByIdAsync(
            request.Id,
            cancellationToken);
        
        if (organization is null)
        {
            return BaseResult.Failure(OrganizationErrors.NotFound);
        }

        var organizationNameResult = OrganizationName.Create(request.Name);

        if (organizationNameResult.IsFailure)
        {
            return BaseResult.Failure(organizationNameResult.Error);
        }

        if (organization.Name != organizationNameResult.Value)
        {
            var exists = await _organizationRepository.ExistsByNameAsync(
                organizationNameResult.Value,
                cancellationToken);

            if (exists)
            {
                return BaseResult.Failure(OrganizationErrors.AlreadyExists);
            }

            organization.UpdateName(organizationNameResult.Value);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BaseResult.Success();
    }
}