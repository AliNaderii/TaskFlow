using TaskFlow.Application.Abstractions.Messaging;

namespace TaskFlow.Application.Comments.Commands.CreateComment;

public sealed record CreateCommentCommand(
    Guid TaskId,
    Guid AuthorUserId,
    string Content)
    : ICommand<Guid>;