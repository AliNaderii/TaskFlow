namespace TaskFlow.Domain.Common;

public interface ITenantEntity
{
    Guid OrganizationId { get; }
}