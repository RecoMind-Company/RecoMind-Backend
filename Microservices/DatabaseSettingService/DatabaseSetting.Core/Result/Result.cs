using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseSetting.Core.Result
{
    public class Result<T>
    {
        private Result(T value)
        {
            Value = value;
            Error = null;
        }
        private Result(Error error)
        {
            Error = error;
            Value = default;
        }

        public T? Value { get; }
        public Error? Error { get; }
        public bool IsSuccess => Error is null;

        public static implicit operator Result<T>(T value) => Success(value);
        public static implicit operator Result<T>(Error error) => Failure(error);
        public static Result<T> Success(T value) => new Result<T>(value);
        public static Result<T> Failure(Error error) => new Result<T>(error);
        public TResult Map<TResult>(
            Func<T, TResult> onSuccess, Func<Error, TResult> onFailure)
            => IsSuccess ? onSuccess(Value!) : onFailure(Error!);
    }
}
