using TaskFlow.Application.Abstractions.Messaging;

namespace TaskFlow.Application.Comments.Commands.ArchiveComment;

public sealed record ArchiveCommentCommand(
    Guid CommentId)
    : ICommand;