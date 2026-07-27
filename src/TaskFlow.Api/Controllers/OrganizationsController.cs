using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Contracts.Organizations;
using TaskFlow.Api.Contracts.Organizations.Invitations;
using TaskFlow.Api.Contracts.Organizations.Membership;
using TaskFlow.Api.Extensions;
using TaskFlow.Application.Organizations.Commands.ArchiveOrganization;
using TaskFlow.Application.Organizations.Commands.CreateOrganization;
using TaskFlow.Application.Organizations.Commands.UpdateOrganization;
using TaskFlow.Application.Organizations.Commands.Invitations.AcceptInvitation;
using TaskFlow.Application.Organizations.Commands.Invitations.CancelInvitation;
using TaskFlow.Application.Organizations.Commands.Invitations.CreateInvitation;
using TaskFlow.Application.Organizations.Commands.Membership.RemoveMember;
using TaskFlow.Application.Organizations.Commands.Membership.LeaveOrganization;
using TaskFlow.Application.Organizations.Commands.Membership.ChangeMemberRole;
using TaskFlow.Application.Organizations.Commands.Membership.SuspendMember;
using TaskFlow.Application.Organizations.Commands.Membership.ActivateMember;
using TaskFlow.Application.Organizations.Queries.GetOrganizationById;
using TaskFlow.Application.Organizations.Queries.Invitations.GetInvitationByToken;
using TaskFlow.Application.Organizations.Queries.Invitations.GetOrganizationInvitations;
namespace TaskFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrganizationsController : ControllerBase
{
    private readonly ISender _sender;
    public OrganizationsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{Id:guid}")]
    public async Task<IActionResult> GetById(
        Guid Id, 
        CancellationToken cancellationToken)
    {
        var query = new GetOrganizationByIdQuery(Id);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToProblemDetails();
        
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateOrganizationCommand(request.Name);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemDetails();
        }

        var response = new CreateOrganizationResponse(result.Value);

        return CreatedAtAction(nameof(Create), new {Id = result.Value}, response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "OrganizationAdmin")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateOrganizationCommand(
            id,
            request.Name);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemDetails();
        }

        return NoContent();
    }

    [HttpPatch("{id:guid}/archive")]
    [Authorize(Policy = "OrganizationAdmin")]
    public async Task<IActionResult> Archive(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new ArchiveOrganizationCommand(id);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemDetails();
        }

        return NoContent();
    }

    [HttpPost("{organizationId:guid}/invitations")]
    [Authorize(Policy = "OrganizationAdmin")]
    public async Task<IActionResult> CreateInvitation(
        Guid organizationId,
        CreateInvitationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateInvitationCommand(
            request.Email,
            request.Role,
            request.ExpirationDays);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemDetails();
        }

        var response = new CreateInvitationResponse(
            result.Value,
            request.Email,
            request.Role.ToString(),
            "Pending",
            DateTime.UtcNow.AddDays(request.ExpirationDays),
            string.Empty);

        return CreatedAtAction(nameof(CreateInvitation), new { organizationId }, response);
    }

    [HttpPost("invitations/accept")]
    [AllowAnonymous]
    public async Task<IActionResult> AcceptInvitation(
        AcceptInvitationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AcceptInvitationCommand(request.Token);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemDetails();
        }

        return Ok(new { InvitationId = result.Value });
    }

    [HttpDelete("{organizationId:guid}/invitations/{invitationId:guid}")]
    [Authorize(Policy = "OrganizationAdmin")]
    public async Task<IActionResult> CancelInvitation(
        Guid organizationId,
        Guid invitationId,
        CancellationToken cancellationToken)
    {
        var command = new CancelInvitationCommand(invitationId);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemDetails();
        }

        return NoContent();
    }

    [HttpGet("invitations/{token}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetInvitationByToken(
        string token,
        CancellationToken cancellationToken)
    {
        var query = new GetInvitationByTokenQuery(token);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemDetails();
        }

        return Ok(result.Value);
    }

    [HttpGet("{organizationId:guid}/invitations")]
    [Authorize(Policy = "OrganizationAdmin")]
    public async Task<IActionResult> GetOrganizationInvitations(
        Guid organizationId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetOrganizationInvitationsQuery(organizationId, page, pageSize);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemDetails();
        }

        return Ok(result.Value);
    }

    [HttpDelete("{organizationId:guid}/members/{userId:guid}")]
    [Authorize(Policy = "OrganizationAdmin")]
    public async Task<IActionResult> RemoveMember(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveMemberCommand(userId);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemDetails();
        }

        return NoContent();
    }

    [HttpPost("{organizationId:guid}/members/leave")]
    public async Task<IActionResult> LeaveOrganization(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var command = new LeaveOrganizationCommand();
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemDetails();
        }

        return NoContent();
    }

    [HttpPut("{organizationId:guid}/members/{userId:guid}/role")]
    [Authorize(Policy = "OrganizationAdmin")]
    public async Task<IActionResult> ChangeMemberRole(
        Guid organizationId,
        Guid userId,
        ChangeMemberRoleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangeMemberRoleCommand(userId, request.Role);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemDetails();
        }

        return NoContent();
    }

    [HttpPut("{organizationId:guid}/members/{userId:guid}/suspend")]
    [Authorize(Policy = "OrganizationAdmin")]
    public async Task<IActionResult> SuspendMember(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var command = new SuspendMemberCommand(userId);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemDetails();
        }

        return NoContent();
    }

    [HttpPut("{organizationId:guid}/members/{userId:guid}/activate")]
    [Authorize(Policy = "OrganizationAdmin")]
    public async Task<IActionResult> ActivateMember(
        Guid organizationId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var command = new ActivateMemberCommand(userId);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemDetails();
        }

        return NoContent();
    }
}
