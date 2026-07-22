namespace TaskFlow.Application.Abstractions.MultiTenancy;

public interface ICurrentTenant
{
    Guid? OrganizationId { get; }
}