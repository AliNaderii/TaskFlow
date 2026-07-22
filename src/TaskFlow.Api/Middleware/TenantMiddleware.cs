using Microsoft.AspNetCore.Http;
using TaskFlow.Application.Abstractions.MultiTenancy;

namespace TaskFlow.Api.Middleware;

public sealed class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(
        RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantContextInitializer tenantContextInitializer)
    {
        await tenantContextInitializer.InitializeAsync();

        await _next(context);
    }
}