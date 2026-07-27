using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.ValueObjects;

public sealed record InvitationToken
{
    public string Value { get; }

    private InvitationToken(string value)
    {
        Value = value;
    }

    public static Result<InvitationToken> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<InvitationToken>.Failure(
                new Error("invitation.token_empty", "Invitation token cannot be empty."));
        }

        if (value.Length < 32)
        {
            return Result<InvitationToken>.Failure(
                new Error("invitation.token_invalid", "Invalid invitation token format."));
        }

        return Result<InvitationToken>.Success(new InvitationToken(value));
    }

    public static InvitationToken Generate()
    {
        var bytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return new InvitationToken(Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", ""));
    }

    public static implicit operator string(InvitationToken token) => token.Value;

    public override string ToString() => Value;
}