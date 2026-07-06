using TaskFlow.Domain.Common;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Errors;

namespace TaskFlow.Domain.ValueObjects;

public sealed record ProjectName
{
    public string Value { get; }

    private ProjectName(string value)
    {
        Value = value;
    }

    public static Result<ProjectName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<ProjectName>.Failure(ProjectErrors.NameIsEmpty);
        }

        value = value.Trim();

        if (value.Length < ProjectConstants.NameMinLength)
        {
            return Result<ProjectName>.Failure(ProjectErrors.NameTooShort);
        }

        if (value.Length > ProjectConstants.NameMaxLength)
        {
            return Result<ProjectName>.Failure(ProjectErrors.NameTooLong);
        }

        return Result<ProjectName>.Success(new ProjectName(value));
    }

    public override string ToString() => Value;
}