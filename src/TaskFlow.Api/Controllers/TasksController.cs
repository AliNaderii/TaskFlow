using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Contracts.Tasks;
using TaskFlow.Api.Extensions;
using TaskFlow.Api.Tasks.Contracts;
using TaskFlow.Application.Tasks.Commands.ArchiveTaskItem;
using TaskFlow.Application.Tasks.Commands.AssignUserToTaskItem;
using TaskFlow.Application.Tasks.Commands.ChangeTaskItemStatus;
using TaskFlow.Application.Tasks.Commands.CreateTaskItem;
using TaskFlow.Application.Tasks.Commands.UnassignUserFromTaskItem;
using TaskFlow.Application.Tasks.Commands.UpdateTaskItem;
using TaskFlow.Application.Tasks.Queries.GetTaskItemById;
using TaskFlow.Application.Tasks.Queries.SearchTaskItems;

namespace TaskFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ISender _sender;

    public TasksController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize(Policy = "OrganizationMember")]
    public async Task<IActionResult> Search(
        [FromQuery] SearchTaskItemsRequest request,
        CancellationToken cancellationToken)
    {
        var query = new SearchTaskItemsQuery(
            request.ProjectId,
            request.Keyword,
            request.Status,
            request.Priority,
            request.AssigneeUserId,
            request.DueDateFrom,
            request.DueDateTo,
            request.IsArchived,
            request.Page,
            request.PageSize,
            request.SortBy,
            request.SortDirection);

        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemDetails();
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [Authorize(Policy = "ProjectManager")]
    public async Task<IActionResult> Create(
        CreateTaskItemRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateTaskItemCommand(
            request.ProjectId,
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate,
            request.AssigneeUserId);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return CreatedAtAction(
            nameof(Create),
            new { id = result.Value },
            result.Value);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "OrganizationMember")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetTaskItemByIdQuery(id);

        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemDetails();
        }

        return Ok(result.Value);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ProjectManager")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateTaskItemRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTaskItemCommand(
            id,
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemDetails();
        }

        return NoContent();
    }

    [HttpPatch("{id:guid}/archive")]
    [Authorize(Policy = "ProjectManager")]
    public async Task<IActionResult> Archive(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new ArchiveTaskItemCommand(id);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemDetails();
        }

        return NoContent();
    }

    [HttpPatch("{id:guid}/assign")]
    [Authorize(Policy = "ProjectManager")]
    public async Task<IActionResult> AssignUser(
        Guid id,
        AssignUserToTaskRequest request,
        CancellationToken cancellationToken)
    {
        var assignedByUserId = User.GetUserId() ?? Guid.Empty;
        
        var command = new AssignUserToTaskItemCommand(
            id,
            request.AssigneeUserId,
            assignedByUserId);

        var result = await _sender.Send(
            command,
            cancellationToken);
        
        if (result.IsFailure)
        {
            return result.Error.ToProblemDetails();
        }

        return NoContent();
    }

    [HttpPatch("{id:guid}/unassign")]
    [Authorize(Policy = "ProjectManager")]
    public async Task<IActionResult> UnassignUser(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new UnassignUserFromTaskItemCommand(id);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemDetails();
        }

        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = "ProjectManager")]
    public async Task<IActionResult> ChangeStatus(
        Guid id,
        ChangeTaskItemStatusRequest request,
        CancellationToken cancellationToken)
    {
        var changedByUserId = User.GetUserId() ?? Guid.Empty;
        
        var command = new ChangeTaskItemStatusCommand(
            id,
            request.Status,
            changedByUserId);

        var result = await _sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemDetails();
        }

        return NoContent();
    }
}