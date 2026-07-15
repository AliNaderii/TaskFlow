using TaskFlow.Application.Abstractions.Messaging;
using TaskFlow.Application.Projects.Queries.GetProjectById;

namespace TaskFlow.Application.Projects.Queries.GetProjectById;

public sealed record GetProjectByIdQuery(Guid Id) : IQuery<ProjectDto>;