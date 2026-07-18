using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Errors;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.Comments.Commands.UpdateComment;

public sealed class UpdateCommentCommandHandler
    : ICommandHandler<UpdateCommentCommand>
{
    private readonly ICommentRepository _commentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCommentCommandHandler(
        ICommentRepository commentRepository,
        IUnitOfWork unitOfWork)
    {
        _commentRepository = commentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseResult> Handle(
        UpdateCommentCommand request, 
        CancellationToken cancellationToken)
    {
        var comment = await _commentRepository.GetByIdAsync(
            request.CommentId,
            cancellationToken);

        if (comment is null)
        {
            return BaseResult.Failure(CommentErrors.NotFound);
        }

        var contentResult = comment.UpdateContent(request.Content);

        if (contentResult.IsFailure)
        {
            return contentResult;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BaseResult.Success();
    }
}