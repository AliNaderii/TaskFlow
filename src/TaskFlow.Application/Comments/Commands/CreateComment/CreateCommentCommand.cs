using TaskFlow.Application.Abstractions.Messaging;

namespace TaskFlow.Application.Comments.Commands.CreateComment;

public sealed record CreateCommentCommand(
    Guid TaskId,
    string Content)
    : ICommand<Guid>;
