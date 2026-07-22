using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Contracts.Organizations;
using TaskFlow.Api.Extensions;
using TaskFlow.Application.Organizations.Commands.ArchiveOrganization;
using TaskFlow.Application.Organizations.Commands.CreateOrganization;
using TaskFlow.Application.Organizations.Commands.UpdateOrganization;
using TaskFlow.Application.Organizations.Queries.GetOrganizationById;
using TaskFlow.Domain.Common;

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
}