using TaskFlow.Domain.Common;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.Constants;

namespace TaskFlow.Domain.ValueObjects;

public sealed record TaskItemTitle
{
    public string Value { get; }

    private TaskItemTitle(string value)
    {
        Value = value;
    }

    public static Result<TaskItemTitle> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<TaskItemTitle>.Failure(
                TaskItemErrors.TitleRequired);
        }

        value = value.Trim();

        if (value.Length < TaskItemConstants.TitleMinLength)
        {
            return Result<TaskItemTitle>.Failure(
                TaskItemErrors.TitleTooShort);
        }

        if (value.Length > TaskItemConstants.TitleMaxLength)
        {
            return Result<TaskItemTitle>.Failure(
                TaskItemErrors.TitleTooLong);
        }

        return Result<TaskItemTitle>.Success(new TaskItemTitle(value));
    }
}