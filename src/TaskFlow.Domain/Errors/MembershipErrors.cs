using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Errors;

public static class MembershipErrors
{
    public static readonly Error NotFound =
        new(
            "Membership.NotFound",
            "Membership does not exist.");
}