using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Abstractions.Messaging;

public interface ICommand : IRequest<BaseResult> { }

public interface ICommand<TResponse> : IRequest<Result<TResponse>> { }