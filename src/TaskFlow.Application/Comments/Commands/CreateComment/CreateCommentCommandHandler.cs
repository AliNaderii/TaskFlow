using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Comments.Commands.CreateComment;

public sealed class CreateCommentCommandHandler
    : ICommandHandler<CreateCommentCommand, Guid>
{
    private readonly ITaskItemRepository _taskItemRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCommentCommandHandler(
        ITaskItemRepository taskItemRepository,
        ICommentRepository commentRepository,
        IUnitOfWork unitOfWork)
    {
        _taskItemRepository = taskItemRepository;
        _commentRepository = commentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        CreateCommentCommand request,
        CancellationToken cancellationToken)
    {
        var taskItem = await _taskItemRepository.GetByIdAsync(
            request.TaskId,
            cancellationToken);

        if (taskItem is null)
        {
            return Result<Guid>.Failure(
                TaskItemErrors.NotFound);
        }

        var contentResult = CommentContent.Create(
            request.Content);

        if (contentResult.IsFailure)
        {
            return Result<Guid>.Failure(
                contentResult.Error);
        }

        var commentResult = Comment.Create(
            request.TaskId,
            request.AuthorUserId,
            contentResult.Value);

        if (commentResult.IsFailure)
        {
            return Result<Guid>.Failure(
                commentResult.Error);
        }

        await _commentRepository.AddAsync(
            commentResult.Value,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<Guid>.Success(
            commentResult.Value.Id);
    }
}