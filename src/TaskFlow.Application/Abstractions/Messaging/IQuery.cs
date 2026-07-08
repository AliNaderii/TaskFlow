using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{

}