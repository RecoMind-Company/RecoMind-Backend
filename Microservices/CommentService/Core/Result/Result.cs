namespace Core.Result;

public class Result<T>
{
    public IEnumerable<Error> ErrorsList { get; set; } = [];
    public T? Value { get; set; }
    public bool IsSuccess => !ErrorsList.Any();
    // constractors
    private Result(T value) => Value = value;
    private Result(IEnumerable<Error> errors) => ErrorsList = errors;
    private Result(Error error) => ErrorsList = [error];
    // factory methods
    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Error error) => new(error);
    public static Result<T> Failure(IEnumerable<Error> errors) => new(errors);
    // mapping method
    public TResult Map<TResult>(Func<T, TResult> onSuccess, Func<IEnumerable<Error>, TResult> onFailure) => IsSuccess ? onSuccess(Value!) : onFailure(ErrorsList);
    public async Task MapAsync(Func<T, Task> onSuccess, Func<IEnumerable<Error>, Task> onFailure)
    {
        if (IsSuccess)
            await onSuccess(Value!);
        else
            await onFailure(ErrorsList);
    }
}
