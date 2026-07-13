using TaskFlow.Domain.Common;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.Constants;

namespace TaskFlow.Domain.ValueObjects;

public sealed record DisplayName
{
    public string Value { get; }

    private DisplayName(string value)
    {
        Value = value;
    }

    public static Result<DisplayName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<DisplayName>.Failure(
                DisplayNameErrors.Required);
        }

        value = value.Trim();

        if (value.Length < DisplayNameConstants.MinLength)
        {
            return Result<DisplayName>.Failure(
                DisplayNameErrors.TooShort);
        }

        if (value.Length > DisplayNameConstants.MaxLength)
        {
            return Result<DisplayName>.Failure(
                DisplayNameErrors.TooLong);
        }

        return Result<DisplayName>.Success(new DisplayName(value));
    }
}