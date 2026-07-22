using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TaskFlow.Application.Abstractions.Authentication;

namespace TaskFlow.Infrastructure.Authentication;

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;


    public CurrentUser(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?.User
                .FindFirstValue(JwtRegisteredClaimNames.Sub);

            return Guid.TryParse( userId, out var id) ? id : null;
        }
    }
}