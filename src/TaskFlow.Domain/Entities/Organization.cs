using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.Constants;

public class Organization : AuditableEntity
{
    private readonly List<Membership> _memberships = new();
    public IReadOnlyCollection<Membership> Memberships => _memberships.AsReadOnly();
    public string Name { get; private set; } = null!;

    private Organization() { }

    private Organization(string name)
    {
        Name = name;
    }

    public static Result<Organization> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<Organization>.Failure(OrganizationErrors.NameIsEmpty);

        if (name.Length > OrganizationConstants.MaxNameLength)
        {
            return Result<Organization>.Failure(OrganizationErrors.NameTooLong);
        }

        return Result<Organization>.Success(new Organization(name));
    }
}