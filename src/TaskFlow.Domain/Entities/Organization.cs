using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.ValueObjects;

public class Organization : AuditableEntity
{
    public OrganizationName Name { get; private set; } = null!;
    
    private readonly List<Membership> _memberships = [];
    public IReadOnlyCollection<Membership> Memberships => _memberships.AsReadOnly();

    private readonly List<Project> _projects = [];
    public IReadOnlyCollection<Project> Projects => _projects.AsReadOnly();


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

    public BaseResult UpdateName(OrganizationName name)
    {
        if (Name == name)
        {
            return BaseResult.Success();
        }
        
        Name = name;
        return BaseResult.Success();
    }

    public BaseResult Archive()
    {
        if (ArchivedAt is not null)
        {
            return BaseResult.Failure(OrganizationErrors.AlreadyArchived);
        }

        ArchivedAt = DateTime.UtcNow;

        return BaseResult.Success();
    }
}