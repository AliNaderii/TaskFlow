using TaskFlow.Domain.Common;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Errors;

namespace TaskFlow.Domain.ValueObjects;

public sealed record ProjectDescription
{
    public string Value { get; }

    private ProjectDescription(string value)
    {
        Value = value;
    }

    public static Result<ProjectDescription?> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<ProjectDescription?>.Success(null);
        }

        value = value.Trim();

        if (value.Length > ProjectConstants.DescriptionMaxLength)
        {
            return Result<ProjectDescription?>.Failure(ProjectErrors.DescriptionTooLong);
        }

        return Result<ProjectDescription?>.Success(new ProjectDescription(value));
    }

    public override string ToString() => Value;
}