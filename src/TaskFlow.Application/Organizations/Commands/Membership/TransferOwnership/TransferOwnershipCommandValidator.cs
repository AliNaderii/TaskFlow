using FluentValidation;

namespace TaskFlow.Application.Organizations.Commands.Membership.TransferOwnership;

public sealed class TransferOwnershipCommandValidator : AbstractValidator<TransferOwnershipCommand>
{
    public TransferOwnershipCommandValidator()
    {
        RuleFor(x => x.TargetUserId)
            .NotEmpty()
            .WithMessage("Target user ID is required.");
    }
}