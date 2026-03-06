namespace Core.Result;

public class Result<T>
{
    private Result(T value) => Value = value;
    private Result(IEnumerable<Error> errors) => ErrorList = errors;


    public T? Value { get; set; }
    public IEnumerable<Error> ErrorList { get; set; } = [];
    public bool IsSuccess => !ErrorList.Any();
    public static Result<T> Success(T value) => new Result<T>(value);
    public static Result<T> Failure(Error error) => new Result<T>(new List<Error> { error });
    public static Result<T> Failure(IEnumerable<Error> errors) => new Result<T>(errors);
    public TResult Map<TResult>(Func<T, TResult> onSuccess, Func<IEnumerable<Error>, TResult> onFailure) => IsSuccess ? onSuccess(Value!) : onFailure(ErrorList);
}
