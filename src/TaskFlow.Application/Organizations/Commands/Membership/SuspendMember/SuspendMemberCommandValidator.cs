using FluentValidation;

namespace TaskFlow.Application.Organizations.Commands.Membership.SuspendMember;

public sealed class SuspendMemberCommandValidator
    : AbstractValidator<SuspendMemberCommand>
{
    public SuspendMemberCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");
    }
}