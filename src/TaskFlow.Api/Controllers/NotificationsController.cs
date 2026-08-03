using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Contracts.Notifications;
using TaskFlow.Application.Notifications.Queries.GetNotifications;
using TaskFlow.Application.Notifications.Queries.GetUnreadCount;
using TaskFlow.Application.Notifications.Commands.MarkAsRead;
using TaskFlow.Application.Notifications.Commands.MarkAllAsRead;
using ApiNotificationResponse = TaskFlow.Api.Contracts.Notifications.NotificationResponse;

namespace TaskFlow.Api.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiController]
[Route("api/[controller]")]
public sealed class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ApiNotificationResponse>>> GetNotifications(
        [FromQuery] GetNotificationsRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetNotificationsQuery(
            request.IsRead,
            request.Page,
            request.PageSize);

        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        var response = result.Value.Select(n => new ApiNotificationResponse(
            n.Id,
            Guid.Empty, // OrganizationId - not in app response
            Guid.Empty, // UserId - not in app response
            n.Type.ToString(),
            n.Title,
            n.Message,
            n.IsRead,
            n.RelatedEntityId,
            n.CreatedAt)).ToList();

        return Ok(response);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount(CancellationToken cancellationToken)
    {
        var query = new GetUnreadCountQuery();
        var result = await _mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpPost("mark-as-read")]
    public async Task<IActionResult> MarkAsRead(
        [FromBody] MarkAsReadRequest request,
        CancellationToken cancellationToken)
    {
        var command = new MarkAsReadCommand(request.NotificationId);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }

    [HttpPost("mark-all-as-read")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        var command = new MarkAllAsReadCommand();
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok();
    }
}