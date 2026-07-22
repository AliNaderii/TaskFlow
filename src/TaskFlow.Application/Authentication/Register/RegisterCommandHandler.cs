using MediatR;
using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Application.Abstractions.Services;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Authentication.Register;

public sealed class RegisterCommandHandler
    : ICommandHandler<RegisterCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;


    public RegisterCommandHandler(
        IUserRepository userRepository,
        IIdentityService identityService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _identityService = identityService;
        _unitOfWork = unitOfWork;
    }


    public async Task<Result<Guid>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var emailResult =
            Email.Create(request.Email);

        if (!emailResult.IsSuccess)
            return Result<Guid>.Failure(emailResult.Error);


        var displayNameResult =
            DisplayName.Create(request.DisplayName);

        if (!displayNameResult.IsSuccess)
            return Result<Guid>.Failure(displayNameResult.Error);


        var exists =
            await _userRepository.ExistsByEmailAsync(
                emailResult.Value,
                cancellationToken);


        if (exists)
        {
            return Result<Guid>.Failure(
                new Error(
                    "User.Email.Exists",
                    "Email is already registered."));
        }


        var userResult =
            User.Create(
                emailResult.Value,
                displayNameResult.Value);


        if (!userResult.IsSuccess)
            return Result<Guid>.Failure(userResult.Error);


        var user = userResult.Value;


        await _userRepository.AddAsync(
            user,
            cancellationToken);


        var identityCreated =
            await _identityService.CreateUserAsync(
                user.Id,
                request.Email,
                request.Password,
                cancellationToken);


        if (!identityCreated)
        {
            return Result<Guid>.Failure(
                new Error(
                    "Identity.CreateFailed",
                    "Unable to create identity user."));
        }


        await _unitOfWork.SaveChangesAsync(cancellationToken);


        return Result<Guid>.Success(user.Id);
    }
}