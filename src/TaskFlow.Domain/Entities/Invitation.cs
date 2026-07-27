using TaskFlow.Domain.Common;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Domain.Entities;

public sealed class Invitation : AuditableEntity, ITenantEntity
{
    public Guid OrganizationId { get; private set; }
    public string Email { get; private set; } = null!;
    public Guid InvitedByUserId { get; private set; }
    public MembershipRole Role { get; private set; }
    public InvitationToken Token { get; private set; } = null!;
    public InvitationStatus Status { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? AcceptedAt { get; private set; }
    public Guid? AcceptedByUserId { get; private set; }
    public User? InvitedBy { get; private set; }
    public Organization? Organization { get; private set; }

    private Invitation() { }

    private Invitation(
        Guid organizationId,
        string email,
        Guid invitedByUserId,
        MembershipRole role,
        InvitationToken token,
        DateTime expiresAt)
    {
        OrganizationId = organizationId;
        Email = email;
        InvitedByUserId = invitedByUserId;
        Role = role;
        Token = token;
        ExpiresAt = expiresAt;
        Status = InvitationStatus.Pending;
    }

    // For EF Core
    private void SetAccepted(Guid acceptedByUserId)
    {
        AcceptedAt = DateTime.UtcNow;
        AcceptedByUserId = acceptedByUserId;
        Status = InvitationStatus.Accepted;
    }

    public static Result<Invitation> Create(
        Guid organizationId,
        string email,
        Guid invitedByUserId,
        MembershipRole role,
        DateTime expiresAt)
    {
        if (organizationId == Guid.Empty)
        {
            return Result<Invitation>.Failure(InvitationErrors.InvalidOrganization);
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return Result<Invitation>.Failure(InvitationErrors.InvalidEmail);
        }

        if (invitedByUserId == Guid.Empty)
        {
            return Result<Invitation>.Failure(InvitationErrors.InvalidInvitedByUser);
        }

        if (expiresAt <= DateTime.UtcNow)
        {
            return Result<Invitation>.Failure(InvitationErrors.InvalidExpiration);
        }

        var token = InvitationToken.Generate();

        var invitation = new Invitation(
            organizationId,
            email.Trim().ToLowerInvariant(),
            invitedByUserId,
            role,
            token,
            expiresAt);

        return Result<Invitation>.Success(invitation);
    }

    public BaseResult Accept(Guid acceptedByUserId)
    {
        if (Status != InvitationStatus.Pending)
        {
            return BaseResult.Failure(InvitationErrors.NotPending);
        }

        if (DateTime.UtcNow > ExpiresAt)
        {
            return BaseResult.Failure(InvitationErrors.Expired);
        }

        SetAccepted(acceptedByUserId);

        return BaseResult.Success();
    }

    public BaseResult Cancel()
    {
        if (Status != InvitationStatus.Pending)
        {
            return BaseResult.Failure(InvitationErrors.NotPending);
        }

        Status = InvitationStatus.Cancelled;

        return BaseResult.Success();
    }

    public BaseResult Expire()
    {
        if (Status != InvitationStatus.Pending)
        {
            return BaseResult.Failure(InvitationErrors.NotPending);
        }

        Status = InvitationStatus.Expired;

        return BaseResult.Success();
    }

    public bool IsValid() => Status == InvitationStatus.Pending && DateTime.UtcNow <= ExpiresAt;
}