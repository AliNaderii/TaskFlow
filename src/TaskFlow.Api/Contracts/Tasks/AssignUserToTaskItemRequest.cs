namespace TaskFlow.Api.Contracts.Tasks;

public sealed record AssignUserToTaskRequest(
    Guid AssigneeUserId);