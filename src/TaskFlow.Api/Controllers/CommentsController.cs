using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Contracts.Comments;
using TaskFlow.Api.Extensions;
using TaskFlow.Application.Comments.Commands.ArchiveComment;
using TaskFlow.Application.Comments.Commands.CreateComment;
using TaskFlow.Application.Comments.Commands.UpdateComment;
using TaskFlow.Application.Comments.Queries.GetCommentById;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/tasks/{taskId:guid}/comments")]
public sealed class CommentsController : ControllerBase
{
    private readonly ISender _sender;

    public CommentsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        Guid taskId,
        CreateCommentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateCommentCommand(
            taskId,
            request.AuthorUserId,
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

    [HttpPut("{commentId:guid}")]
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