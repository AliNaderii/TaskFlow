using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.ValueObjects;

public class Organization : AuditableEntity
{
    private readonly List<Membership> _memberships = new();
    public IReadOnlyCollection<Membership> Memberships => _memberships.AsReadOnly();
    public OrganizationName Name { get; private set; }

    private Organization() { }

    private Organization(OrganizationName name)
    {
        Name = name;
    }

    public static Result<Organization> Create(OrganizationName name)
    {
        var organization = new Organization(name);
        return Result<Organization>.Success(organization);
    }
}