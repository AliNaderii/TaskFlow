using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Abstractions.MultiTenancy;

namespace TaskFlow.Api.Middleware;

public sealed class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(
        RequestDelegate next,
        ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantContextInitializer tenantContextInitializer)
    {
        _logger.LogDebug("TenantMiddleware.InvokeAsync for {Path}", context.Request.Path);

        try
        {
            await tenantContextInitializer.InitializeAsync();
            _logger.LogDebug("TenantMiddleware: InitializeAsync completed successfully for {Path}", context.Request.Path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TenantMiddleware: Exception in InitializeAsync for {Path}", context.Request.Path);
            // Don't throw, let the request continue
        }

        try
        {
            await _next(context);
            _logger.LogDebug("TenantMiddleware: _next completed for {Path}", context.Request.Path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TenantMiddleware: Exception in _next for {Path}", context.Request.Path);
            throw;
        }
    }
}
