using TaskFlow.Application.Abstractions.Messaging;

namespace TaskFlow.Application.Comments.Commands.UpdateComment;

public sealed record UpdateCommentCommand(
    Guid CommentId,
    string Content)
    : ICommand;