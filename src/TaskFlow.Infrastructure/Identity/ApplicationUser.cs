using Microsoft.AspNetCore.Identity;

namespace TaskFlow.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public Guid DomainUserId { get; set; }
}