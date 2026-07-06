using TaskFlow.Domain.Common;
using TaskFlow.Domain.ValueObjects;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.Constants;

namespace TaskFlow.Domain.Entities;

public class User : AuditableEntity
{
    public Email Email { get; private set; } = null!;
    public DisplayName DisplayName { get; private set; } = null!;
    public bool IsActive { get; private set; }

    private User()
    {
        // Required by EF
    }

    private User( Email email, DisplayName displayName)
    {
        Email = email;
        DisplayName = displayName;
        IsActive = true;
    }

    public static Result<User> Create(Email email, DisplayName displayName)
    {
        return Result<User>.Success(new User(email, displayName));
    }

    public BaseResult ChangeDisplayName(DisplayName displayName)
    {
        if (DisplayName.Value == displayName.Value?.Trim())
            return BaseResult.Success();

        DisplayName = displayName;

        return BaseResult.Success();
    }

    public BaseResult DeActivate()
    {
        if (!IsActive)
            return BaseResult.Success();

        IsActive = false;

        return BaseResult.Success();
    }

    public BaseResult Activate()
    {
        if (IsActive)
            return BaseResult.Success();

        IsActive = true;

        return BaseResult.Success();
    }
}