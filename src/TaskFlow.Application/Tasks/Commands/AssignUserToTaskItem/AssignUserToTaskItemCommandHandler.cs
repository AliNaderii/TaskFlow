using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Abstractions.Persistence;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.Errors;

namespace TaskFlow.Application.Tasks.Commands.AssignUserToTaskItem;

public sealed class AssignUserToTaskCommandHandler
    : ICommandHandler<AssignUserToTaskItemCommand>
{
    private readonly ITaskItemRepository _taskItemRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignUserToTaskCommandHandler(
        ITaskItemRepository taskItemRepository,
        IUserRepository userRepository,
        IProjectRepository projectRepository,
        IMembershipRepository membershipRepository,
        IUnitOfWork unitOfWork)
    {
        _taskItemRepository = taskItemRepository;
        _userRepository = userRepository;
        _projectRepository = projectRepository;
        _membershipRepository = membershipRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseResult> Handle(
        AssignUserToTaskItemCommand request,
        CancellationToken cancellationToken)
    {
        var taskItem = await _taskItemRepository.GetByIdAsync(
            request.TaskItemId,
            cancellationToken);

        if (taskItem is null)
        {
            return BaseResult.Failure(TaskItemErrors.NotFound);
        }

        var user = await _userRepository.GetByIdAsync(
            request.AssigneeUserId,
            cancellationToken);

        if (user is null)
        {
            return BaseResult.Failure(UserErrors.NotFound);
        }

        var project = await _projectRepository.GetByIdAsync(
            taskItem.ProjectId,
            cancellationToken);

        if (project is null)
        {
            return BaseResult.Failure(ProjectErrors.NotFound);
        }

        var isMember = await _membershipRepository.ExistsAsync(
            request.AssigneeUserId,
            project.OrganizationId,
            cancellationToken);

        if (!isMember)
        {
            return BaseResult.Failure(MembershipErrors.NotFound);
        }

        var result = taskItem.AssignTo(request.AssigneeUserId);

        if (result.IsFailure)
        {
            return result;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BaseResult.Success();
    }
}