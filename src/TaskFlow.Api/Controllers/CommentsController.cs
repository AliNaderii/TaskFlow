using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Contracts.Comments;
using TaskFlow.Api.Extensions;
using TaskFlow.Application.Comments.Commands.ArchiveComment;
using TaskFlow.Application.Comments.Commands.CreateComment;
using TaskFlow.Application.Comments.Commands.UpdateComment;
using TaskFlow.Application.Comments.Queries.GetCommentById;
using TaskFlow.Application.Comments.Queries.GetCommentsByTaskId;
using TaskFlow.Application.Comments.Queries.SearchComments;

namespace TaskFlow.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/tasks/{taskId:guid}/comments")]
public sealed class CommentsController : ControllerBase
{
    private readonly ISender _sender;

    public CommentsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("search")]
    [Authorize(Policy = "OrganizationMember")]
    public async Task<IActionResult> Search(
        Guid taskId,
        [FromQuery] SearchCommentsRequest request,
        CancellationToken cancellationToken)
    {
        var query = new SearchCommentsQuery(
            taskId,
            request.Keyword,
            request.AuthorUserId,
            request.CreatedAtFrom,
            request.CreatedAtTo,
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
        Guid taskId,
        CreateCommentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCommentCommand(
            taskId,
            request.Content);

        var result = await _sender.Send(
            command,
            cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemDetails();
        }

        return CreatedAtAction(
            nameof(Create),
            new { id = result.Value },
            result.Value);
    }

    [HttpGet("{commentId:guid}")]
    [Authorize(Policy = "OrganizationMember")]
    public async Task<IActionResult> GetById(
        Guid taskId,
        Guid commentId,
        CancellationToken cancellationToken)
    {
        var query = new GetCommentByIdQuery(commentId);

        var result = await _sender.Send(
            query,
            cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemDetails();
        }

        return Ok(result.Value);
    }

    [HttpGet]
    [Authorize(Policy = "OrganizationMember")]
    public async Task<IActionResult> GetByTaskId(
        Guid taskId,
        CancellationToken cancellationToken)
    {
        var query = new GetCommentsByTaskIdQuery(taskId);

        var result = await _sender.Send(
            query,
            cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemDetails();
        }

        return Ok(result.Value);
    }

    [HttpPut("{commentId:guid}")]
    [Authorize(Policy = "ProjectManager")]
    public async Task<IActionResult> Update(
        Guid taskId,
        Guid commentId,
        UpdateCommentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCommentCommand(commentId, request.Content);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemDetails();
        }

        return NoContent();
    }

    [HttpDelete("{commentId:guid}")]
    [Authorize(Policy = "ProjectManager")]
    public async Task<IActionResult> Archive(
        Guid taskId,
        Guid commentId,
        CancellationToken cancellationToken)
    {
        var command = new ArchiveCommentCommand(commentId);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemDetails();
        }

        return NoContent();
    }
}