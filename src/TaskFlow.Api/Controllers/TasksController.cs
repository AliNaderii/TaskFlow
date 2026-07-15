using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Tasks.Contracts;
using TaskFlow.Application.Tasks.Commands.CreateTaskItem;

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
}