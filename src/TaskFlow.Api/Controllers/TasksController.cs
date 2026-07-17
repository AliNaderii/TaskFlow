using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Contracts.Tasks;
using TaskFlow.Api.Extensions;
using TaskFlow.Api.Tasks.Contracts;
using TaskFlow.Application.Tasks.Commands.CreateTaskItem;
using TaskFlow.Application.Tasks.Commands.UpdateTaskItem;
using TaskFlow.Application.Tasks.Queries.GetTaskItemById;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ISender _sender;

    public TasksController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateTaskItemRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateTaskItemCommand(
            request.ProjectId,
            request.CreatorUserId,
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
}