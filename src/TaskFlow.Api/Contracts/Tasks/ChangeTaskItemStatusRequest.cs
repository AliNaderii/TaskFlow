using TaskFlow.Domain.Enums;

namespace TaskFlow.Api.Contracts.Tasks;

public sealed record ChangeTaskItemStatusRequest(
    TaskItemStatus Status);