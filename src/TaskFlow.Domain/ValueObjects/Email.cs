using System.Net.Mail;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.ValueObjects;

public sealed record Email
{
    public string Value { get; }

    private Email (string value)
    {
        Value = value;
    }

    public static Result<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<Email>.Failure(EmailErrors.Empty);
        }

        value = value.Trim().ToLowerInvariant();

        try
        {
            _ = new MailAddress(value);
        }
        catch
        {
            return Result<Email>.Failure(EmailErrors.Invalid);
        }

        return Result<Email>.Success(new Email(value));
    }

    public override string ToString() => Value;
}