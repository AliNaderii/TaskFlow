using Microsoft.AspNetCore.Builder;
using TaskFlow.Api.Middleware;

namespace TaskFlow.Api.Extensions;

public static class TenantMiddlewareExtensions
{
    public static IApplicationBuilder UseTenant(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantMiddleware>();
    }
}