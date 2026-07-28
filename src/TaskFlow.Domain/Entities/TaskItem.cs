using TaskFlow.Domain.Common;
using TaskFlow.Domain.Events;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.ValueObjects;
using TaskFlow.Domain.Errors;

namespace TaskFlow.Domain.Entities;

public sealed class TaskItem : AuditableEntity, ITenantEntity
{
    public Guid OrganizationId {get; private set;}
    public Guid ProjectId { get; private set; }
    public Guid CreatorUserId { get; private set; }
    public Guid? AssigneeUserId { get; private set; }
    public TaskItemTitle Title { get; private set; } = null!;
    public TaskItemDescription? Description { get; private set; }
    public TaskItemStatus Status { get; private set; }
    public TaskItemPriority Priority { get; private set; }
    public DateTime? DueDate { get; private set; }
    public bool IsArchived { get; private set; }
    public Project Project { get; private set; } = null!;
    public User Creator { get; private set; } = null!;
    public User? Assignee { get; private set; }

    private readonly List<Comment> _comments = [];
    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();


    private TaskItem() { }

    private TaskItem(
        Guid organizationId,
        Guid projectId,
        Guid creatorUserId,
        TaskItemTitle title,
        TaskItemDescription? description,
        TaskItemPriority priority,
        DateTime? dueDate,
        Guid? assigneeUserId)
    {
        OrganizationId = organizationId;
        ProjectId = projectId;
        CreatorUserId = creatorUserId;
        Title = title;
        Description = description;
        Priority = priority;
        DueDate = dueDate;
        AssigneeUserId = assigneeUserId;

        Status = TaskItemStatus.Todo;
        IsArchived = false;
    }

    public static Result<TaskItem> Create(
        Guid organizationId,
        Guid projectId,
        Guid creatorUserId,
        TaskItemTitle title,
        TaskItemDescription? description,
        TaskItemPriority priority,
        DateTime? dueDate,
        Guid? assigneeUserId)
    {
        if (creatorUserId == Guid.Empty)
        {
            return Result<TaskItem>.Failure(TaskItemErrors.InvalidCreatorUserId);
        }

        if (organizationId == Guid.Empty)
        {
            return Result<TaskItem>.Failure(TaskItemErrors.InvalidOrganizationId);
        }

        var taskItem = new TaskItem(
            organizationId,
            projectId,
            creatorUserId,
            title,
            description,
            priority,
            dueDate,
            assigneeUserId);

        return Result<TaskItem>.Success(taskItem);
    }

    public BaseResult Rename(string title)
    {
        var result = TaskItemTitle.Create(title);

        if (result.IsFailure)
        {
            return BaseResult.Failure(result.Error);
        }

        Title = result.Value;

        return BaseResult.Success();
    }

    public BaseResult ChangeDescription(string? description)
    {
        var result = TaskItemDescription.Create(description);

        if (result.IsFailure)
        {
            return BaseResult.Failure(result.Error);
        }

        Description = result.Value;
        
        return BaseResult.Success();
    }

    public BaseResult ChangePriority(TaskItemPriority priority)
    {
        Priority = priority;
        return BaseResult.Success();
    }

    public BaseResult ChangeDueDate(DateTime? dueDate)
    {
        DueDate = dueDate;
        return BaseResult.Success();
    }

    public BaseResult AssignTo(Guid userId, Guid assignedByUserId)
    {
        if (IsArchived)
        {
            return BaseResult.Failure(TaskItemErrors.AlreadyArchived);
        }
    
        if (AssigneeUserId == userId)
        {
            return BaseResult.Failure(TaskItemErrors.AlreadyAssignedToUser);
        }
    
        AssigneeUserId = userId;
        
        AddDomainEvent(TaskAssignedEvent.Create(Id, userId, assignedByUserId));
    
        return BaseResult.Success();
    }
    
        public BaseResult Unassign()
    {
        if (IsArchived)
        {
            return BaseResult.Failure(TaskItemErrors.AlreadyArchived);
        }
    
        if (AssigneeUserId is null)
        {
            return BaseResult.Failure(TaskItemErrors.NotAssigned);
        }
    
        AssigneeUserId = null;
    
        return BaseResult.Success();
    }
    
    public BaseResult ChangeStatus(TaskItemStatus status, Guid changedByUserId)
    {
        if (IsArchived)
        {
            return BaseResult.Failure(TaskItemErrors.AlreadyArchived);
        }
    
        if (Status == status)
        {
            return BaseResult.Failure(TaskItemErrors.StatusAlreadySet);
        }
    
        Status = status;
        
        if (status == TaskItemStatus.Done)
        {
            AddDomainEvent(TaskCompletedEvent.Create(Id, changedByUserId));
        }
    
        return BaseResult.Success();
    }

    public BaseResult Archive()
    {
        if (IsArchived)
        {
            return BaseResult.Failure(TaskItemErrors.AlreadyArchived);
        }

        IsArchived = true;

        return BaseResult.Success();
    }

    public BaseResult Restore()
    {
        if (!IsArchived)
        {
            return BaseResult.Failure(TaskItemErrors.NotArchived);
        }

        IsArchived = false;

        return BaseResult.Success();
    }
}