using TaskFlow.Domain.Common;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.Constants;

namespace TaskFlow.Domain.ValueObjects;

public sealed record CommentContent
{
    public string Value { get; }

    private CommentContent(string value)
    {
        Value = value;
    }

    public static Result<CommentContent> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<CommentContent>.Failure(CommentErrors.ContentRequired);
        }

        value = value.Trim();

        if (value.Length < CommentConstants.ContentMinLength)
        {
            return Result<CommentContent>.Failure(CommentErrors.ContentTooShort);
        }

        if (value.Length > CommentConstants.ContentMaxLength)
        {
            return Result<CommentContent>.Failure(CommentErrors.ContentTooLong);
        }

        return Result<CommentContent>.Success(new CommentContent(value));
    }

    public override string ToString() => Value;
}