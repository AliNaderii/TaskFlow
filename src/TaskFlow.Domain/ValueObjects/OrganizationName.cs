using TaskFlow.Domain.Common;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Errors;

namespace TaskFlow.Domain.ValueObjects;

public sealed record OrganizationName
{
    public string Value { get; }

    private OrganizationName(string value)
    {
        Value = value;
    }

    public static Result<OrganizationName> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<OrganizationName>.Failure(OrganizationErrors.NameRequired);
        }

        value = value.Trim();

        if (value.Length < OrganizationConstants.NameMinLength)
        {
            return Result<OrganizationName>.Failure(OrganizationErrors.NameTooShort);
        }

        if (value.Length > OrganizationConstants.NameMaxLength)
        {
            return Result<OrganizationName>.Failure(OrganizationErrors.NameTooLong);
        }

        return Result<OrganizationName>.Success(new OrganizationName(value));
    }

    public override string ToString() => Value;
}