using Microsoft.AspNetCore.Authorization;
using TaskFlow.Application.Abstractions.Authentication;
using TaskFlow.Application.Abstractions.Authorization;
using TaskFlow.Application.Abstractions.MultiTenancy;

namespace TaskFlow.Infrastructure.Authorization;

public sealed class OrganizationMemberHandler
    : AuthorizationHandler<OrganizationMemberRequirement>
{
    private readonly ICurrentUser _currentUser;
    private readonly ICurrentTenant _currentTenant;
    private readonly IAppAuthorizationService _appAuthorizationService;

    public OrganizationMemberHandler(
        ICurrentUser currentUser,
        ICurrentTenant currentTenant,
        IAppAuthorizationService appAuthorizationService)
    {
        _currentUser = currentUser;
        _currentTenant = currentTenant;
        _appAuthorizationService = appAuthorizationService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrganizationMemberRequirement requirement)
    {
        if (_currentUser.Id is null || _currentTenant.OrganizationId is null)
        {
            context.Fail();
            return;
        }

        var isMember = await _appAuthorizationService.IsMemberAsync(
            _currentUser.Id.Value,
            _currentTenant.OrganizationId.Value);

        if (isMember)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }
}