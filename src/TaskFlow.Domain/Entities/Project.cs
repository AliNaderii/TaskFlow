using TaskFlow.Domain.Common;
using TaskFlow.Domain.ValueObjects;
using TaskFlow.Domain.Errors;

namespace TaskFlow.Domain.Entities;

public sealed class Project : AuditableEntity
{
    public Guid OrganizationId { get; private set; }

    public ProjectName Name { get; private set; } = null!;

    public ProjectDescription? Description { get; private set; }

    public bool IsArchived { get; private set; }

    private Project()
    {
    }

    private Project(
        Guid organizationId,
        ProjectName name,
        ProjectDescription? description)
    {
        OrganizationId = organizationId;
        Name = name;
        Description = description;
    }

    public static Result<Project> Create(
        Guid organizationId,
        ProjectName name,
        ProjectDescription? description)
    {
        var project = new Project(
            organizationId,
            name,
            description);

        return Result<Project>.Success(project);
    }

    public BaseResult Rename(ProjectName name)
    {
        if (Name == name)
        {
            return BaseResult.Success();
        }

        Name = name;

        return BaseResult.Success();
    }

    public BaseResult ChangeDescription(ProjectDescription? description)
    {
        if (Description == description)
        {
            return BaseResult.Success();
        }

        Description = description;

        return BaseResult.Success();
    }

    public BaseResult Archive()
    {
        if (IsArchived)
        {
            return BaseResult.Failure(ProjectErrors.AlreadyArchived);
        }

        IsArchived = true;

        return BaseResult.Success();
    }

    public BaseResult Restore()
    {
        if (!IsArchived)
        {
            return BaseResult.Failure(ProjectErrors.AlreadyActive);
        }

        IsArchived = false;

        return BaseResult.Success();
    }
}