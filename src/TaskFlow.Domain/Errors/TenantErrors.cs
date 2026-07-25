using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Errors;

public static class TenantErrors
{
    public static readonly Error NotFound =
        new(
            "Tenant.NotFound",
            "Current tenant was not found.");
}