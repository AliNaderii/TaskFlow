using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Abstractions.Messaging;

public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, BaseResult>
    where TCommand : ICommand
{

}

public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{

}