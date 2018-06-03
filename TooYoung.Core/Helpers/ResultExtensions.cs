using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace TooYoung.Core.Helpers
{
    public static class ResultExtensions
    {
        public static Result<TNewValue, TError> Flatten<TValue, TNewValue, TError>(this Result<TValue, TError> result,
            Func<TValue, Result<TNewValue, TError>> func) where TError : class
        {
            if (result.IsFailure)
                return Result.Fail<TNewValue, TError>(result.Error);

            return func(result.Value);
        }

        public static Result<K> Flatten<T, K>(this Result<T> result, Func<T, Result<K>> func)
        {
            if (result.IsFailure)
                return Result.Fail<K>(result.Error);

            return func(result.Value);
        }

        public static Result<T> Flatten<T>(this Result result, Func<Result<T>> func)
        {
            if (result.IsFailure)
                return Result.Fail<T>(result.Error);

            return func();
        }

        #region Async in both operands
        public static async Task<Result<K>> Flatten<T, K>(this Task<Result<T>> resultTask, Func<T, Task<Result<K>>> func, bool continueOnCapturedContext = true)
        {
            Result<T> result = await resultTask.ConfigureAwait(continueOnCapturedContext);

            if (result.IsFailure)
                return Result.Fail<K>(result.Error);

            var flattenped = await func(result.Value).ConfigureAwait(continueOnCapturedContext);

            return flattenped;
        }

        public static async Task<Result<T>> Flatten<T>(this Task<Result> resultTask, Func<Task<Result<T>>> func, bool continueOnCapturedContext = true)
        {
            Result result = await resultTask.ConfigureAwait(continueOnCapturedContext);

            if (result.IsFailure)
                return Result.Fail<T>(result.Error);

            var flattenped = await func().ConfigureAwait(continueOnCapturedContext);

            return flattenped;
        }
        #endregion

        #region Async in right operand
        public static async Task<Result<K>> Flatten<T, K>(this Result<T> result, Func<T, Task<Result<K>>> func, bool continueOnCapturedContext = true)
        {
            if (result.IsFailure)
                return Result.Fail<K>(result.Error);

            var flattenped = await func(result.Value).ConfigureAwait(continueOnCapturedContext);

            return flattenped;
        }

        public static async Task<Result<T>> Flatten<T>(this Result result, Func<Task<Result<T>>> func, bool continueOnCapturedContext = true)
        {
            if (result.IsFailure)
                return Result.Fail<T>(result.Error);

            var flattenped = await func().ConfigureAwait(continueOnCapturedContext);

            return flattenped;
        }
        #endregion

        #region Async in left operand
        public static async Task<Result<K>> Flatten<T, K>(this Task<Result<T>> resultTask, Func<T, Result<K>> func, bool continueOnCapturedContext = true)
        {
            Result<T> result = await resultTask.ConfigureAwait(continueOnCapturedContext);
            return result.Flatten(func);
        }

        public static async Task<Result<T>> Flatten<T>(this Task<Result> resultTask, Func<Result<T>> func, bool continueOnCapturedContext = true)
        {
            Result result = await resultTask.ConfigureAwait(continueOnCapturedContext);
            return result.Flatten(func);
        }
        #endregion
    }
}
