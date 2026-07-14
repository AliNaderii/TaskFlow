using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Contracts.Organizations;
using TaskFlow.Api.Extensions;
using TaskFlow.Application.Organizations.Commands.CreateOrganization;
using TaskFlow.Application.Organizations.Queries.GetOrganizationById;

namespace TaskFlow.Api.Controllers;

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

        return CreatedAtAction(nameof(Create), response);
    }
}