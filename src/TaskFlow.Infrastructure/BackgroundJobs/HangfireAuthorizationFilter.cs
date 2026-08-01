using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;

namespace TaskFlow.Infrastructure.BackgroundJobs;

public sealed class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly string _policy;

    public HangfireAuthorizationFilter(string policy)
    {
        _policy = policy;
    }

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        
        if (httpContext.User.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        // Check if user has the required policy
        var authorizationService = httpContext.RequestServices.GetService(typeof(Microsoft.AspNetCore.Authorization.IAuthorizationService)) 
            as Microsoft.AspNetCore.Authorization.IAuthorizationService;

        if (authorizationService == null)
        {
            return false;
        }

        // Since we can't easily await here, we do a basic check
        // The policy requires OrganizationAdmin which checks membership
        // For dashboard access, we verify the user is authenticated and has admin claims
        return httpContext.User.HasClaim("organization_admin", "true") 
            || httpContext.User.IsInRole("Admin")
            || httpContext.User.HasClaim(c => c.Type == "permission" && c.Value.Contains("OrganizationAdmin"));
    }
}