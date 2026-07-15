using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Extensions;
using TaskFlow.Application.Projects.Queries.GetProjectById;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly ISender _sender;

    public ProjectsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new GetProjectByIdQuery(id);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemDetails();
        }

        return Ok(result.Value);
    }
}