using Microsoft.AspNetCore.Http;
using System.IO;
using TaskFlow.Application.Abstractions.MultiTenancy;

namespace TaskFlow.Api.Middleware;

public sealed class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly string LogPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "taskflow_tenant_debug.log");

    public TenantMiddleware(
        RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantContextInitializer tenantContextInitializer)
    {
        var logMsg = $"[DEBUG] TenantMiddleware.InvokeAsync for {context.Request.Path}";
        System.IO.File.AppendAllText(LogPath, logMsg + Environment.NewLine);
        System.Console.WriteLine(logMsg);
        
        try
        {
            await tenantContextInitializer.InitializeAsync();
            System.IO.File.AppendAllText(LogPath, $"[DEBUG] TenantMiddleware: InitializeAsync completed successfully" + Environment.NewLine);
            System.Console.WriteLine($"[DEBUG] TenantMiddleware: InitializeAsync completed successfully for {context.Request.Path}");
        }
        catch (Exception ex)
        {
            System.IO.File.AppendAllText(LogPath, $"[DEBUG] TenantMiddleware: Exception in InitializeAsync: {ex}" + Environment.NewLine);
            System.Console.WriteLine($"[DEBUG] TenantMiddleware: Exception in InitializeAsync: {ex}");
            // Don't throw, let the request continue to see what happens
            // throw;
        }

        try
        {
            await _next(context);
            System.IO.File.AppendAllText(LogPath, $"[DEBUG] TenantMiddleware: _next completed for {context.Request.Path}" + Environment.NewLine);
            System.Console.WriteLine($"[DEBUG] TenantMiddleware: _next completed for {context.Request.Path}");
        }
        catch (Exception ex)
        {
            System.IO.File.AppendAllText(LogPath, $"[DEBUG] TenantMiddleware: Exception in _next: {ex}" + Environment.NewLine);
            System.Console.WriteLine($"[DEBUG] TenantMiddleware: Exception in _next: {ex}");
            throw;
        }
    }
}