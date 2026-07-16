using TaskFlow.Application.Abstractions.Messaging;

namespace TaskFlow.Application.Tasks.Queries.GetTaskItemById;

public sealed record GetTaskItemByIdQuery(Guid Id)
    : IQuery<TaskItemDto>;