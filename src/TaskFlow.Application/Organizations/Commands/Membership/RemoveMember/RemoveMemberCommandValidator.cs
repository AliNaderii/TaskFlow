using FluentValidation;

namespace TaskFlow.Application.Organizations.Commands.Membership.RemoveMember;

public sealed class RemoveMemberCommandValidator
    : AbstractValidator<RemoveMemberCommand>
{
    public RemoveMemberCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");
    }
}