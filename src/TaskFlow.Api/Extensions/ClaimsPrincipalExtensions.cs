using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace TaskFlow.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return Guid.TryParse(userId, out var id) ? id : null;
    }
}