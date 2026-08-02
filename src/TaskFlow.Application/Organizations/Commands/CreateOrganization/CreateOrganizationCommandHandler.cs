using TaskFlow.Application.Abstractions.Authentication;
using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Organizations.Commands.CreateOrganization;

public sealed class CreateOrganizationCommandHandler
    : ICommandHandler<CreateOrganizationCommand, Guid>
{
    private readonly IOrganizationRepository _repository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public CreateOrganizationCommandHandler(
        IOrganizationRepository repository,
        IMembershipRepository membershipRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _membershipRepository = membershipRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
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

        var organization = createOrganizationResult.Value;

        await _repository.AddAsync(organization, cancellationToken);
        
        // Create membership for the creator as Owner
        var userId = _currentUser.Id ?? throw new InvalidOperationException("User must be authenticated to create an organization");
        
        var membershipResult = Domain.Entities.Membership.Create(userId, organization.Id, MembershipRole.Owner);
        if (membershipResult.IsFailure)
        {
            return Result<Guid>.Failure(membershipResult.Error);
        }

        await _membershipRepository.AddAsync(membershipResult.Value, cancellationToken);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(organization.Id);
    }
}