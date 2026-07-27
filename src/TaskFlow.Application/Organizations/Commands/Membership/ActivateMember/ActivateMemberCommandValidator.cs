using FluentValidation;

namespace TaskFlow.Application.Organizations.Commands.Membership.ActivateMember;

public sealed class ActivateMemberCommandValidator
    : AbstractValidator<ActivateMemberCommand>
{
    public ActivateMemberCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");
    }
}