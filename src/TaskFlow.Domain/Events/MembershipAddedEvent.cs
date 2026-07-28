using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Events;

public sealed record MembershipAddedEvent : IDomainEvent
{
    public Guid MembershipId { get; init; }
    public Guid OrganizationId { get; init; }
    public Guid UserId { get; init; }
    public string Role { get; init; } = string.Empty;
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public Guid EventId { get; init; } = Guid.NewGuid();

    private MembershipAddedEvent() { }

    public static MembershipAddedEvent Create(Guid membershipId, Guid organizationId, Guid userId, string role)
    {
        return new MembershipAddedEvent
        {
            MembershipId = membershipId,
            OrganizationId = organizationId,
            UserId = userId,
            Role = role
        };
    }
}