using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Organizations.Commands.CreateOrganization;

public sealed class CreateOrganizationCommandHandler
    : ICommandHandler<CreateOrganizationCommand, Guid>
{
    private readonly IOrganizationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrganizationCommandHandler(
        IOrganizationRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateOrganizationCommand request, 
        CancellationToken cancellationToken)
    {
        var organizationNameResult = OrganizationName.Create(request.Name);

        if (organizationNameResult.IsFailure)
        {
            return Result<Guid>.Failure(organizationNameResult.Error);
        }

        if (await _repository.ExistsByNameAsync(
                organizationNameResult.Value,
                cancellationToken))
        {
            return Result<Guid>.Failure(OrganizationErrors.AlreadyExists);
        }

        var createOrganizationResult = Organization.Create(organizationNameResult.Value);
        
        if (createOrganizationResult.IsFailure)
        {
            return Result<Guid>.Failure(createOrganizationResult.Error);
        }

        await _repository.AddAsync(
            createOrganizationResult.Value,
            cancellationToken);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(createOrganizationResult.Value.Id);
    }
}