namespace TaskFlow.Domain.Common;

public class BaseResult
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    protected BaseResult(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new ArgumentException("Cannot have success with error");

        if (!isSuccess && error == Error.None)
            throw new ArgumentException("Failure must have an error");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static BaseResult Success() => new(true, Error.None);
    public static BaseResult Failure(Error error) => new(false, error);
}

