using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.MultiTenancy;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Application.Abstractions.Authentication;
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
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    public CreateCommentCommandHandler(
        ITaskItemRepository taskItemRepository,
        ICommentRepository commentRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _taskItemRepository = taskItemRepository;
        _commentRepository = commentRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(
        CreateCommentCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentTenant.OrganizationId.HasValue)
        {
            return Result<Guid>.Failure(TenantErrors.NotFound);
        }

        if (!_currentUser.Id.HasValue)
        {
            return Result<Guid>.Failure(new Error("auth.user_not_found", "User not authenticated."));
        }

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
            _currentTenant.OrganizationId.Value,
            request.TaskId,
            _currentUser.Id.Value,
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
