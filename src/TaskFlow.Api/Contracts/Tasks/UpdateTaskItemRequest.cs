using TaskFlow.Domain.Enums;

namespace TaskFlow.Api.Contracts.Tasks;

public sealed record UpdateTaskItemRequest(
    string Title,
    string Description,
    TaskItemPriority Priority,
    DateTime? DueDate);