using System.ComponentModel.DataAnnotations;
using FluentValidation;
using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : BaseResult
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

	public async Task<TResponse> Handle(
		TRequest request,
		RequestHandlerDelegate<TResponse> next,
		CancellationToken cancellationToken)
	{
		if (!_validators.Any())
            return await next();
        
        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        
        
        var errors = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (errors.Count == 0)
            return await next();

        var firstError = errors.First();

        return (TResponse)BaseResult.Failure(
            new Error(firstError.ErrorCode,firstError.ErrorMessage));
    }
}