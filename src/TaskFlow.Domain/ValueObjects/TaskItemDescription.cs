using TaskFlow.Domain.Common;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Errors;

namespace TaskFlow.Domain.ValueObjects;

public sealed record TaskItemDescription
{
    public string Value { get; }

    private TaskItemDescription(string value)
    {
        Value = value;
    }

    public static Result<TaskItemDescription?> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<TaskItemDescription?>.Success(null);
        }

        value = value.Trim();

        if (value.Length > TaskItemConstants.DescriptionMaxLength)
        {
            return Result<TaskItemDescription?>.Failure(TaskItemErrors.DescriptionTooLong);
        }

        return Result<TaskItemDescription?>.Success(new TaskItemDescription(value));
    }

    public override string ToString() => Value;
}