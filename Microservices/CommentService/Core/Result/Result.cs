namespace Core.Result;

public class Result<T>
{
    public IEnumerable<Error> ErrorsList { get; set; } = [];
    public T Value { get; set; }
    public bool IsSuccess => !ErrorsList.Any();
    private Result(T value)
    {
        Value = value;
    }
    private Result(IEnumerable<Error> errors) => ErrorsList = errors;
    private Result(Error error) => ErrorsList = new List<Error> { error };
    public static Result<T> Success(T value) => new Result<T>(value);
    public static Result<T> Failure(Error error) => new Result<T>(error);
    public static Result<T> Failure(IEnumerable<Error> errors) => new Result<T>(errors);
    public TResult Map<TResult>(Func<T, TResult> onSuccess, Func<IEnumerable<Error>, TResult> onFailure) => IsSuccess ? onSuccess(Value) : onFailure(ErrorsList);
}
