using Stoq.Shared.Kernel.Enums;

namespace Stoq.Shared.Kernel.Common;

public sealed class Result<TValue>
{
    private readonly TValue? _value;
    private readonly string? _error;
    private readonly ResultStatus _status;

    private Result(TValue value)
    {
        _status = ResultStatus.Success;
        _value = value;
    }

    private Result(string error, ResultStatus status)
    {
        _status = status;
        _error = error;
    }

    public bool IsSuccess => _status == ResultStatus.Success;
    public bool IsFailure => _status == ResultStatus.Failure;
    public bool IsForbidden => _status == ResultStatus.Forbidden;
    public bool IsNotFound => _status == ResultStatus.NotFound;

    public TValue Value =>
        IsSuccess ? _value! : throw new InvalidOperationException("Cannot access Value on a failed result.");

    public string Error =>
        !IsSuccess ? _error! : throw new InvalidOperationException("Cannot access Error on a successful result.");

    public static Result<TValue> Success(TValue value) => new(value);
    public static Result<TValue> Failure(string error) => new(error, ResultStatus.Failure);
    public static Result<TValue> Forbidden(string error) => new(error, ResultStatus.Forbidden);
    public static Result<TValue> NotFound(string error) => new(error, ResultStatus.NotFound);

    public TOut Match<TOut>(Func<TValue, TOut> onSuccess, Func<string, TOut> onFailure) =>
        IsSuccess ? onSuccess(_value!) : onFailure(_error!);
}

public sealed class Result
{
    private readonly string? _error;
    private readonly ResultStatus _status;

    private Result() => _status = ResultStatus.Success;

    private Result(string error, ResultStatus status)
    {
        _status = status;
        _error = error;
    }

    public bool IsSuccess => _status == ResultStatus.Success;
    public bool IsFailure => _status == ResultStatus.Failure;
    public bool IsForbidden => _status == ResultStatus.Forbidden;
    public bool IsNotFound => _status == ResultStatus.NotFound;

    public string Error =>
        !IsSuccess ? _error! : throw new InvalidOperationException("Cannot access Error on a successful result.");

    public static Result Success() => new();
    public static Result Failure(string error) => new(error, ResultStatus.Failure);
    public static Result Forbidden(string error) => new(error, ResultStatus.Forbidden);
    public static Result NotFound(string error) => new(error, ResultStatus.NotFound);
}
