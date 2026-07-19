using System.Reflection;
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
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var errors = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (errors.Count == 0)
        {
            return await next();
        }

        var firstError = errors[0];
        var errorCode = string.IsNullOrWhiteSpace(firstError.ErrorCode)
            ? "Validation.Error"
            : firstError.ErrorCode;

        var error = new Error(errorCode, firstError.ErrorMessage);

        return CreateFailure(error);
    }

    private static TResponse CreateFailure(Error error)
    {
        if (typeof(TResponse) == typeof(BaseResult))
        {
            return (TResponse)BaseResult.Failure(error);
        }

        if (typeof(TResponse).IsGenericType
            && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            // Result<T>.Failure(Error) — construct the typed failure, not BaseResult.
            var failureMethod = typeof(TResponse).GetMethod(
                nameof(Result<object>.Failure),
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly,
                binder: null,
                types: [typeof(Error)],
                modifiers: null)
                ?? throw new InvalidOperationException(
                    $"Could not find Failure method on '{typeof(TResponse).FullName}'.");

            return (TResponse)failureMethod.Invoke(null, [error])!;
        }

        throw new InvalidOperationException(
            $"Validation failure cannot be created for response type '{typeof(TResponse).FullName}'.");
    }
}
