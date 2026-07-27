using FluentValidation;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Organizations.Commands.Membership.ChangeMemberRole;

public sealed class ChangeMemberRoleCommandValidator
    : AbstractValidator<ChangeMemberRoleCommand>
{
    public ChangeMemberRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.Role)
            .IsInEnum()
            .WithMessage("Invalid membership role.");
    }
}