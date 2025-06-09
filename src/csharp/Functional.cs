using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace common;

#pragma warning disable CA1716 // Identifiers should not match keywords
/// <summary>
/// Represents an optional value that may or may not contain a value.
/// </summary>
public sealed record Option<T>
#pragma warning restore CA1716 // Identifiers should not match keywords
{
    private readonly T? value;
    private readonly bool isSome;

    private Option()
    {
        isSome = false;
    }

    private Option(T value)
    {
        this.value = value;
        isSome = true;
    }

    /// <summary>
    /// Gets whether this option contains no value.
    /// </summary>
    public bool IsNone => !isSome;

    /// <summary>
    /// Gets whether this option contains a value.
    /// </summary>
    public bool IsSome => isSome;

#pragma warning disable CA1000 // Do not declare static members on generic types
    internal static Option<T> Some(T value) =>
        new(value);

    /// <summary>
    /// Creates an empty option.
    /// </summary>
    /// <returns>An option representing no value.</returns>
    public static Option<T> None() =>
        new();
#pragma warning restore CA1000 // Do not declare static members on generic types

    public override string ToString() =>
        IsSome ? $"Some({value})" : "None";

    public bool Equals(Option<T>? other) =>
        (this, other) switch
        {
            (_, null) => false,
            ({ IsSome: true }, { IsSome: true }) => EqualityComparer<T?>.Default.Equals(value, other.value),
            ({ IsNone: true }, { IsNone: true }) => true,
            _ => false
        };

    public override int GetHashCode() =>
        value is null
        ? 0
        : EqualityComparer<T?>.Default.GetHashCode(value);

    /// <summary>
    /// Pattern matches on the option state.
    /// </summary>
    /// <typeparam name="T2">The return type.</typeparam>
    /// <param name="some">Function executed if the option contains a value.</param>
    /// <param name="none">Function executed if the option is empty.</param>
    /// <returns>The result of the executed function.</returns>
    public T2 Match<T2>(Func<T, T2> some, Func<T2> none) =>
        IsSome ? some(value!) : none();

    /// <summary>
    /// Pattern matches on the option state for side effects.
    /// </summary>
    /// <param name="some">Action executed if the option contains a value.</param>
    /// <param name="none">Action executed if the option is empty.</param>
    public void Match(Action<T> some, Action none)
    {
        if (IsSome)
            some(value!);
        else
            none();
    }

    /// <summary>
    /// Implicitly converts a value to Some(value).
    /// </summary>
    public static implicit operator Option<T>(T value) =>
        Some(value);

    /// <summary>
    /// Implicitly converts None to an empty option.
    /// </summary>
    public static implicit operator Option<T>(None _) =>
        None();
}

/// <summary>
/// Represents the absence of a value in an option.
/// </summary>
public readonly record struct None
{
    public override string ToString() =>
        "None";

    public override int GetHashCode() => 0;
}

#pragma warning disable CA1716 // Identifiers should not match keywords
public static class Option
#pragma warning restore CA1716 // Identifiers should not match keywords
{
    /// <summary>
    /// Creates an option containing a value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>Some(value).</returns>
    public static Option<T> Some<T>(T value) =>
        Option<T>.Some(value);

    /// <summary>
    /// A None value for creating empty options.
    /// </summary>
    public static None None { get; }

    /// <summary>
    /// Filters an option using a predicate.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The option to filter.</param>
    /// <param name="predicate">The predicate function.</param>
    /// <returns>The option if it satisfies the predicate, otherwise None.</returns>
    public static Option<T> Where<T>(this Option<T> option, Func<T, bool> predicate) =>
        option.Match(t => predicate(t) ? option : None,
                     () => None);

    /// <summary>
    /// Transforms the option value using a function.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="T2">The result value type.</typeparam>
    /// <param name="option">The option to transform.</param>
    /// <param name="f">The transformation function.</param>
    /// <returns>Some(f(value)) if Some, otherwise None.</returns>
    public static Option<T2> Map<T, T2>(this Option<T> option, Func<T, T2> f) =>
        option.Match(t => Some(f(t)),
                     () => None);

    /// <summary>
    /// Chains option operations together (monadic bind).
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="T2">The result value type.</typeparam>
    /// <param name="option">The option to bind.</param>
    /// <param name="f">The function that returns an option.</param>
    /// <returns>f(value) if Some, otherwise None.</returns>
    public static Option<T2> Bind<T, T2>(this Option<T> option, Func<T, Option<T2>> f) =>
        option.Match(t => f(t),
                     () => None);

    /// <summary>
    /// Projects the option value (LINQ support).
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="T2">The result value type.</typeparam>
    /// <param name="option">The option to project.</param>
    /// <param name="f">The projection function.</param>
    /// <returns>The projected option.</returns>
    public static Option<T2> Select<T, T2>(this Option<T> option, Func<T, T2> f) =>
        option.Map(f);

    /// <summary>
    /// Projects and flattens nested options (LINQ support).
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="T2">The intermediate value type.</typeparam>
    /// <typeparam name="TResult">The result value type.</typeparam>
    /// <param name="option">The source option.</param>
    /// <param name="f">The function that returns an intermediate option.</param>
    /// <param name="selector">The result selector function.</param>
    /// <returns>The flattened result option.</returns>
    public static Option<TResult> SelectMany<T, T2, TResult>(this Option<T> option, Func<T, Option<T2>> f,
                                                         Func<T, T2, TResult> selector) =>
        option.Bind(t => f(t).Map(t2 => selector(t, t2)));

    /// <summary>
    /// Provides a fallback value for empty options.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The option to check.</param>
    /// <param name="f">Function that provides the default value.</param>
    /// <returns>The option value if Some, otherwise the default value.</returns>
    public static T IfNone<T>(this Option<T> option, Func<T> f) =>
        option.Match(t => t,
                     f);

    /// <summary>
    /// Provides a fallback option for empty options.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The option to check.</param>
    /// <param name="f">Function that provides the fallback option.</param>
    /// <returns>The original option if Some, otherwise the fallback.</returns>
    public static Option<T> IfNone<T>(this Option<T> option, Func<Option<T>> f) =>
        option.Match(t => option,
                     f);

    /// <summary>
    /// Extracts the option value or throws an exception.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The option to check.</param>
    /// <param name="getException">Function that creates the exception to throw.</param>
    /// <returns>The option value.</returns>
    /// <exception cref="Exception">Thrown when the option is None.</exception>
    public static T IfNoneThrow<T>(this Option<T> option, Func<Exception> getException) =>
        option.Match(t => t,
                     () => throw getException());

    /// <summary>
    /// Converts the option to a nullable reference type.
    /// </summary>
    /// <typeparam name="T">The reference type.</typeparam>
    /// <param name="option">The option to convert.</param>
    /// <returns>The option value if Some, otherwise null.</returns>
    public static T? IfNoneNull<T>(this Option<T> option) where T : class =>
    option.Match(t => (T?)t,
                 () => null);

    /// <summary>
    /// Converts the option to a nullable value type.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The option to convert.</param>
    /// <returns>The option value if Some, otherwise null.</returns>
    public static T? IfNoneNullable<T>(this Option<T> option) where T : struct =>
        option.Match(t => (T?)t,
                     () => null);

    /// <summary>
    /// Executes an action if the option contains a value.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The option to check.</param>
    /// <param name="f">The action to execute.</param>
    public static void Iter<T>(this Option<T> option, Action<T> f) =>
        option.Match(f,
                     () => { });

    /// <summary>
    /// Executes an async action if the option contains a value.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="option">The option to check.</param>
    /// <param name="f">The async action to execute.</param>
    /// <returns>A task representing the async operation.</returns>
    public static async ValueTask IterTask<T>(this Option<T> option, Func<T, ValueTask> f) =>
        await option.Match<ValueTask>(async t => await f(t),
                                      () => ValueTask.CompletedTask);
}


/// <summary>
/// Represents the result of an operation that can either succeed with a value or fail with an error.
/// </summary>
public sealed record Result<T>
{
    private readonly T? value;
    private readonly Error? error;
    private readonly bool isSuccess;

    private Result(T value)
    {
        this.value = value;
        isSuccess = true;
    }

    private Result(Error error)
    {
        this.error = error;
        isSuccess = false;
    }

    /// <summary>
    /// Gets whether this result represents a success.
    /// </summary>
    public bool IsSuccess => isSuccess;

    /// <summary>
    /// Gets whether this result represents an error.
    /// </summary>
    public bool IsError => isSuccess is false;

#pragma warning disable CA1000 // Do not declare static members on generic types
    internal static Result<T> Success(T value) =>
        new(value);

    internal static Result<T> Error(Error error) =>
        new(error);
#pragma warning restore CA1000 // Do not declare static members on generic types

    /// <summary>
    /// Pattern matches on the result state.
    /// </summary>
    /// <typeparam name="TResult">The return type.</typeparam>
    /// <param name="onSuccess">Function executed if the result is successful.</param>
    /// <param name="onError">Function executed if the result is an error.</param>
    /// <returns>The result of the executed function.</returns>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onError) =>
        IsSuccess ? onSuccess(value!) : onError(error!);

    /// <summary>
    /// Pattern matches on the result state for side effects.
    /// </summary>
    /// <param name="onSuccess">Action executed if the result is successful.</param>
    /// <param name="onError">Action executed if the result is an error.</param>
    public void Match(Action<T> onSuccess, Action<Error> onError)
    {
        if (IsSuccess)
        {
            onSuccess(value!);
        }
        else
        {
            onError(error!);
        }
    }

    public override string ToString() =>
        IsSuccess ? $"Success: {value}" : $"Error: {error}";

    public bool Equals(Result<T>? other) =>
        (this, other) switch
        {
            (_, null) => false,
            ({ IsSuccess: true }, { IsSuccess: true }) =>
                EqualityComparer<T?>.Default.Equals(value, other.value),
            ({ IsError: true }, { IsError: true }) =>
                EqualityComparer<Error?>.Default.Equals(error, other.error),
            _ => false
        };

    public override int GetHashCode() =>
        HashCode.Combine(value, error);

    /// <summary>
    /// Implicitly converts a value to Success(value).
    /// </summary>
    public static implicit operator Result<T>(T value) =>
        Success(value);

    /// <summary>
    /// Implicitly converts an error to Error(error).
    /// </summary>
    public static implicit operator Result<T>(Error error) =>
        Error(error);
}

/// <summary>
/// Provides static methods for creating and working with Result instances.
/// </summary>
public static class Result
{
    /// <summary>
    /// Creates a successful result containing a value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>Success(value).</returns>
    public static Result<T> Success<T>(T value) =>
        Result<T>.Success(value);

    /// <summary>
    /// Creates an error result containing an error.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="error">The error to wrap.</param>
    /// <returns>Error(error).</returns>
    public static Result<T> Error<T>(Error error) =>
        Result<T>.Error(error);

    /// <summary>
    /// Transforms the success value using a function.
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="T2">The result value type.</typeparam>
    /// <param name="result">The result to transform.</param>
    /// <param name="f">The transformation function.</param>
    /// <returns>Success(f(value)) if successful, otherwise the original error.</returns>
    public static Result<T2> Map<T, T2>(this Result<T> result, Func<T, T2> f) =>
        result.Match(value => Success(f(value)),
                     error => Error<T2>(error));

    /// <summary>
    /// Transforms the error, preserving any success value.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="result">The result to transform.</param>
    /// <param name="f">The error transformation function.</param>
    /// <returns>The original success, or Error(f(error)) if error.</returns>
    public static Result<T> MapError<T>(this Result<T> result, Func<Error, Error> f) =>
        result.Match(value => Success(value),
                     error => Error<T>(f(error)));

    /// <summary>
    /// Chains result operations together (monadic bind).
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="T2">The result value type.</typeparam>
    /// <param name="result">The result to bind.</param>
    /// <param name="f">The function that returns a result.</param>
    /// <returns>f(value) if successful, otherwise the original error.</returns>
    public static Result<T2> Bind<T, T2>(this Result<T> result, Func<T, Result<T2>> f) =>
        result.Match(value => f(value),
                     error => Error<T2>(error));

    /// <summary>
    /// Projects the result value (LINQ support).
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="T2">The result value type.</typeparam>
    /// <param name="result">The result to project.</param>
    /// <param name="f">The projection function.</param>
    /// <returns>The projected result.</returns>
    public static Result<T2> Select<T, T2>(this Result<T> result, Func<T, T2> f) =>
        result.Map(f);

    /// <summary>
    /// Projects and flattens nested results (LINQ support).
    /// </summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <typeparam name="T2">The intermediate value type.</typeparam>
    /// <typeparam name="TResult">The result value type.</typeparam>
    /// <param name="result">The source result.</param>
    /// <param name="f">The function that returns an intermediate result.</param>
    /// <param name="selector">The result selector function.</param>
    /// <returns>The flattened result.</returns>
    public static Result<TResult> SelectMany<T, T2, TResult>(this Result<T> result, Func<T, Result<T2>> f,
                                                             Func<T, T2, TResult> selector) =>
        result.Bind(value => f(value)
              .Map(value2 => selector(value, value2)));

    /// <summary>
    /// Provides a fallback value for error results.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="result">The result to check.</param>
    /// <param name="f">Function that provides the fallback value.</param>
    /// <returns>The success value if successful, otherwise the fallback value.</returns>
    public static T IfError<T>(this Result<T> result, Func<Error, T> f) =>
        result.Match(value => value,
                     f);

    /// <summary>
    /// Provides a fallback result for error results.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="result">The result to check.</param>
    /// <param name="f">Function that provides the fallback result.</param>
    /// <returns>The original result if successful, otherwise the fallback.</returns>
    public static Result<T> IfError<T>(this Result<T> result, Func<Error, Result<T>> f) =>
        result.Match(_ => result,
                     f);

    /// <summary>
    /// Executes an action if the result is successful.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="result">The result to check.</param>
    /// <param name="f">The action to execute.</param>
    public static void Iter<T>(this Result<T> result, Action<T> f) =>
        result.Match(f,
                     _ => { });

    /// <summary>
    /// Executes an async action if the result is successful.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="result">The result to check.</param>
    /// <param name="f">The async action to execute.</param>
    /// <returns>A task representing the async operation.</returns>
    public static async ValueTask IterTask<T>(this Result<T> result, Func<T, ValueTask> f) =>
        await result.Match<ValueTask>(async value => await f(value),
                                      _ => ValueTask.CompletedTask);

    /// <summary>
    /// Extracts the success value or throws the error as an exception.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="result">The result to check.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="Exception">Thrown when the result is an error.</exception>
    public static T IfErrorThrow<T>(this Result<T> result) =>
        result.Match(value => value,
                     error => throw error.ToException());

    /// <summary>
    /// Converts the result to a nullable reference type.
    /// </summary>
    /// <typeparam name="T">The reference type.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>The success value if successful, otherwise null.</returns>
    public static T? IfErrorNull<T>(this Result<T> result) where T : class =>
        result.Match(value => (T?)value,
                     _ => null);

    /// <summary>
    /// Converts the result to a nullable value type.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>The success value if successful, otherwise null.</returns>
    public static T? IfErrorNullable<T>(this Result<T> result) where T : struct =>
        result.Match(value => (T?)value,
                     _ => null);

    /// <summary>
    /// Converts a result to an option, discarding error information.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>Some(value) if the result is successful, otherwise None.</returns>
    public static Option<T> ToOption<T>(this Result<T> result) =>
        result.Match(Option.Some, _ => Option.None);
}

#pragma warning disable CA1716 // Identifiers should not match keywords
/// <summary>
/// Represents an error containing one or more messages.
/// </summary>
public record Error
#pragma warning restore CA1716 // Identifiers should not match keywords
{
    private readonly ImmutableHashSet<string> messages;

    protected Error(IEnumerable<string> messages)
    {
        this.messages = [.. messages];
    }

    /// <summary>
    /// Gets all error messages as an immutable set.
    /// </summary>
    public ImmutableHashSet<string> Messages => messages;

    /// <summary>
    /// Creates an error from one or more messages.
    /// </summary>
    /// <param name="messages">The error messages.</param>
    /// <returns>An error containing the specified messages.</returns>
    public static Error From(params string[] messages) =>
        new(messages);

    /// <summary>
    /// Creates an error from an exception.
    /// </summary>
    /// <param name="exception">The exception to wrap.</param>
    /// <returns>An exceptional error containing the exception.</returns>
    public static Error From(Exception exception) =>
        new Exceptional(exception);

    /// <summary>
    /// Converts the error to an appropriate exception.
    /// </summary>
    /// <returns>An exception representing this error.</returns>
    public virtual Exception ToException() =>
        messages.ToArray() switch
        {
            [var message] => new InvalidOperationException(message),
            _ => new AggregateException(messages.Select(message => new InvalidOperationException(message)))
        };

    public override string ToString() =>
        messages.ToArray() switch
        {
            [var message] => message,
            _ => string.Join("; ", messages)
        };

    /// <summary>
    /// Implicitly converts a string to an error.
    /// </summary>
    public static implicit operator Error(string message) =>
        From(message);

    /// <summary>
    /// Implicitly converts an exception to an error.
    /// </summary>
    public static implicit operator Error(Exception exception) =>
        From(exception);

    /// <summary>
    /// Combines two errors into a single error.
    /// </summary>
    /// <param name="left">The first error.</param>
    /// <param name="right">The second error.</param>
    /// <returns>An error containing messages from both errors.</returns>
    public static Error operator +(Error left, Error right) =>
        (left.messages, right.messages) switch
        {
            ({ Count: 0 }, _) => right,
            (_, { Count: 0 }) => left,
            _ => new(left.messages.Union(right.messages))
        };

    public virtual bool Equals(Error? other) =>
        (this, other) switch
        {
            (_, null) => false,
            ({ messages.Count: 0 }, { messages.Count: 0 }) => true,
            _ => messages.SetEquals(other.messages)
        };

    public override int GetHashCode() =>
        messages.Count switch
        {
            0 => 0,
            _ => messages.Aggregate(0, (hash, message) => HashCode.Combine(hash, message.GetHashCode()))
        };

    /// <summary>
    /// Represents an error that wraps an exception.
    /// </summary>
    public sealed record Exceptional : Error
    {
        internal Exceptional(Exception exception) : base([exception.Message])
        {
            Exception = exception;
        }

        /// <summary>
        /// Gets the wrapped exception.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Returns the original wrapped exception.
        /// </summary>
        /// <returns>The original exception.</returns>
        public override Exception ToException() => Exception;

        public bool Equals(Error.Exceptional? other) =>
            (this, other) switch
            {
                (_, null) => false,
                _ => Exception.Equals(other.Exception)
            };

        public override int GetHashCode() =>
            Exception.GetHashCode();
    }
}

/// <summary>
/// Provides extension methods for working with IEnumerable&lt;T&gt; in a functional style.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Gets the first element of the sequence as an option.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <returns>Some(first element) if the sequence has elements, otherwise None.</returns>
    public static Option<T> Head<T>(this IEnumerable<T> source)
    {
        using var enumerator = source.GetEnumerator();

        return enumerator.MoveNext()
                ? Option.Some(enumerator.Current)
                : Option.None;
    }

    /// <summary>
    /// Filters and transforms elements using an option-returning selector.
    /// </summary>
    /// <typeparam name="T">The source element type.</typeparam>
    /// <typeparam name="T2">The result element type.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="selector">Function that returns an option for each element.</param>
    /// <returns>A sequence containing only the values where the selector returned Some.</returns>
    public static IEnumerable<T2> Choose<T, T2>(this IEnumerable<T> source, Func<T, Option<T2>> selector) =>
        source.Select(selector)
              .Where(option => option.IsSome)
              .Select(option => option.IfNone(() => throw new UnreachableException("All options should be in the 'Some' state.")));

    /// <summary>
    /// Finds the first element that produces a Some value when transformed.
    /// </summary>
    /// <typeparam name="T">The source element type.</typeparam>
    /// <typeparam name="T2">The result element type.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="selector">Function that returns an option for each element.</param>
    /// <returns>The first Some value produced by the selector, or None if no element produces Some.</returns>
    public static Option<T2> Pick<T, T2>(this IEnumerable<T> source, Func<T, Option<T2>> selector) =>
        source.Select(selector)
              .Where(option => option.IsSome)
              .DefaultIfEmpty(Option.None)
              .First();

   /// <summary>
   /// Applies a result-returning function to each element, collecting successes or aggregating errors.
   /// </summary>
   /// <typeparam name="T">The source element type.</typeparam>
   /// <typeparam name="T2">The result element type.</typeparam>
   /// <param name="source">The source enumerable.</param>
   /// <param name="selector">Function that returns a result for each element.</param>
   /// <param name="cancellationToken">Cancellation token.</param>
   /// <returns>Success with all results if all succeed, otherwise an error with all failures combined.</returns>
   public static Result<ImmutableArray<T2>> Traverse<T, T2>(this IEnumerable<T> source, Func<T, Result<T2>> selector, CancellationToken cancellationToken)
    {
        var results = new List<T2>();
        var errors = new List<Error>();

        source.Iter(item => selector(item).Match(results.Add, errors.Add),
                    maxDegreeOfParallelism: 1,
                    cancellationToken);

        return errors.Count > 0
                ? errors.Aggregate((first, second) => first + second)
                : Result.Success(results.ToImmutableArray());
    }

    /// <summary>
    /// Applies an option-returning function to each element, succeeding only if all elements succeed.
    /// </summary>
    /// <typeparam name="T">The source element type.</typeparam>
    /// <typeparam name="T2">The result element type.</typeparam>
    /// <param name="source">The source enumerable.</param>
    /// <param name="selector">Function that returns an option for each element.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Some with all results if all succeed, otherwise None.</returns>
    public static Option<ImmutableArray<T2>> Traverse<T, T2>(this IEnumerable<T> source, Func<T, Option<T2>> selector, CancellationToken cancellationToken)
    {
        var results = new List<T2>();
        var hasNone = false;

        source.Iter(item => selector(item).Match(results.Add, () => hasNone = true),
                    maxDegreeOfParallelism: 1,
                    cancellationToken);

        return hasNone
                ? Option.None
                : Option.Some(results.ToImmutableArray());
    }

    /// <summary>
    /// Executes an action on each element in parallel.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source enumerable.</param>
    /// <param name="action">The action to execute for each element.</param>
    /// <param name="maxDegreeOfParallelism">Maximum degree of parallelism.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static void Iter<T>(this IEnumerable<T> source, Action<T> action, Option<int> maxDegreeOfParallelism, CancellationToken cancellationToken)
    {
        var options = new ParallelOptions { CancellationToken = cancellationToken };
        maxDegreeOfParallelism.Iter(max => options.MaxDegreeOfParallelism = max);

        Parallel.ForEach(source, options, item => action(item));
    }

    /// <summary>
    /// Executes an async action on each element in parallel.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source enumerable.</param>
    /// <param name="action">The async action to execute for each element.</param>
    /// <param name="maxDegreeOfParallelism">Maximum degree of parallelism.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the async operation.</returns>
    public static async ValueTask IterTask<T>(this IEnumerable<T> source, Func<T, ValueTask> action, Option<int> maxDegreeOfParallelism, CancellationToken cancellationToken)
    {
        var options = new ParallelOptions { CancellationToken = cancellationToken };
        maxDegreeOfParallelism.Iter(max => options.MaxDegreeOfParallelism = max);

        await Parallel.ForEachAsync(source, options, async (item, _) => await action(item));
    }

    /// <summary>
    /// Executes a side effect action on each element as it's enumerated.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source enumerable.</param>
    /// <param name="action">The side effect action to execute.</param>
    /// <returns>The original enumerable unchanged, useful for debugging or logging without affecting data flow.</returns>
    public static IEnumerable<T> Tap<T>(this IEnumerable<T> source, Action<T> action) =>
        source.Select(item =>
        {
            action(item);
            return item;
        });

    /// <summary>
    /// Separates an enumerable of tuples into a tuple of immutable arrays.
    /// </summary>
    /// <typeparam name="T1">The first tuple element type.</typeparam>
    /// <typeparam name="T2">The second tuple element type.</typeparam>
    /// <param name="source">The source enumerable of tuples.</param>
    /// <returns>A tuple containing two immutable arrays with the separated elements.</returns>
    public static (ImmutableArray<T1>, ImmutableArray<T2>) Unzip<T1, T2>(this IEnumerable<(T1, T2)> source)
    {
        var list1 = new List<T1>();
        var list2 = new List<T2>();

        foreach (var (item1, item2) in source)
        {
            list1.Add(item1);
            list2.Add(item2);
        }

        return ([.. list1], [.. list2]);
    }
}

/// <summary>
/// Provides extension methods for working with IAsyncEnumerable&lt;T&gt; in a functional style.
/// </summary>
public static class AsyncEnumerableExtensions
{
    /// <summary>
    /// Gets the first element of the async sequence as an option.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source async sequence.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Some(first element) if the sequence has elements, otherwise None.</returns>
    public static async ValueTask<Option<T>> Head<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken)
    {
        await using var enumerator = source.GetAsyncEnumerator(cancellationToken);

        return await enumerator.MoveNextAsync()
                ? Option.Some(enumerator.Current)
                : Option.None;
    }

    /// <summary>
    /// Filters and transforms async elements using an option-returning selector.
    /// </summary>
    /// <typeparam name="T">The source element type.</typeparam>
    /// <typeparam name="T2">The result element type.</typeparam>
    /// <param name="source">The source async sequence.</param>
    /// <param name="selector">Function that returns an option for each element.</param>
    /// <returns>An async sequence containing only the values where the selector returned Some.</returns>
    public static IAsyncEnumerable<T2> Choose<T, T2>(this IAsyncEnumerable<T> source, Func<T, Option<T2>> selector) =>
        source.Select(selector)
              .Where(option => option.IsSome)
              .Select(option => option.IfNone(() => throw new UnreachableException("All options should be in the 'Some' state.")));

    /// <summary>
    /// Finds the first async element that produces a Some value when transformed.
    /// </summary>
    /// <typeparam name="T">The source element type.</typeparam>
    /// <typeparam name="T2">The result element type.</typeparam>
    /// <param name="source">The source async sequence.</param>
    /// <param name="selector">Function that returns an option for each element.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The first Some value produced by the selector, or None if no element produces Some.</returns>
    public static async ValueTask<Option<T2>> Pick<T, T2>(this IAsyncEnumerable<T> source, Func<T, Option<T2>> selector, CancellationToken cancellationToken) =>
        await source.Select(selector)
                    .Where(option => option.IsSome)
                    .DefaultIfEmpty(Option.None)
                    .FirstAsync(cancellationToken);

   /// <summary>
   /// Applies an async result-returning function to each element, collecting successes or aggregating errors.
   /// </summary>
   /// <typeparam name="T">The source element type.</typeparam>
   /// <typeparam name="T2">The result element type.</typeparam>
   /// <param name="source">The source async enumerable.</param>
   /// <param name="selector">Async function that returns a result for each element.</param>
   /// <param name="cancellationToken">Cancellation token.</param>
   /// <returns>Success with all results if all succeed, otherwise an error with all failures combined.</returns>
   public static async ValueTask<Result<ImmutableArray<T2>>> Traverse<T, T2>(this IAsyncEnumerable<T> source, Func<T, ValueTask<Result<T2>>> selector, CancellationToken cancellationToken)
    {
        var results = new List<T2>();
        var errors = new List<Error>();

        await source.IterTask(async item =>
                              {
                                  var result = await selector(item);
                                  result.Match(results.Add, errors.Add);
                              },
                              maxDegreeOfParallelism: 1,
                              cancellationToken);

        return errors.Count > 0
                ? errors.Aggregate((first, second) => first + second)
                : Result.Success(results.ToImmutableArray());
    }

    /// <summary>
    /// Applies an async option-returning function to each element, succeeding only if all elements succeed.
    /// </summary>
    /// <typeparam name="T">The source element type.</typeparam>
    /// <typeparam name="T2">The result element type.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <param name="selector">Async function that returns an option for each element.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Some with all results if all succeed, otherwise None.</returns>
    public static async ValueTask<Option<ImmutableArray<T2>>> Traverse<T, T2>(this IAsyncEnumerable<T> source, Func<T, ValueTask<Option<T2>>> selector, CancellationToken cancellationToken)
    {
        var results = new List<T2>();
        var hasNone = false;

        await source.IterTask(async item =>
                              {
                                  var option = await selector(item);
                                  option.Match(results.Add, () => hasNone = true);
                              },
                              maxDegreeOfParallelism: 1,
                              cancellationToken);

        return hasNone
                ? Option.None
                : Option.Some(results.ToImmutableArray());
    }

    /// <summary>
    /// Executes an async action on each element in parallel.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <param name="action">The async action to execute for each element.</param>
    /// <param name="maxDegreeOfParallelism">Maximum degree of parallelism.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the async operation.</returns>
    public static async ValueTask IterTask<T>(this IAsyncEnumerable<T> source, Func<T, ValueTask> action, Option<int> maxDegreeOfParallelism, CancellationToken cancellationToken)
    {
        var options = new ParallelOptions { CancellationToken = cancellationToken };
        maxDegreeOfParallelism.Iter(max => options.MaxDegreeOfParallelism = max);

        await Parallel.ForEachAsync(source, options, async (item, _) => await action(item));
    }

    /// <summary>
    /// Executes a side effect action on each async element as it's enumerated.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <param name="action">The side effect action to execute.</param>
    /// <returns>The original async enumerable unchanged, useful for debugging or logging without affecting data flow.</returns>
    public static IAsyncEnumerable<T> Tap<T>(this IAsyncEnumerable<T> source, Action<T> action) =>
        source.Select(item =>
        {
            action(item);
            return item;
        });

    /// <summary>
    /// Executes an async side effect action on each async element as it's enumerated.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source async enumerable.</param>
    /// <param name="action">The async side effect action to execute.</param>
    /// <returns>The original async enumerable unchanged, useful for debugging or logging without affecting data flow.</returns>
    public static IAsyncEnumerable<T> TapTask<T>(this IAsyncEnumerable<T> source, Func<T, ValueTask> action) =>
        source.Select(async (T item, CancellationToken _) =>
        {
            await action(item);
            return item;
        });

    /// <summary>
    /// Separates an async enumerable of tuples into a tuple of immutable arrays.
    /// </summary>
    /// <typeparam name="T1">The first tuple element type.</typeparam>
    /// <typeparam name="T2">The second tuple element type.</typeparam>
    /// <param name="source">The source async enumerable of tuples.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple containing two immutable arrays with the separated elements.</returns>
    public static async ValueTask<(ImmutableArray<T1>, ImmutableArray<T2>)> Unzip<T1, T2>(this IAsyncEnumerable<(T1, T2)> source, CancellationToken cancellationToken)
    {
        var list1 = new List<T1>();
        var list2 = new List<T2>();

        await foreach (var (item1, item2) in source.WithCancellation(cancellationToken))
        {
            list1.Add(item1);
            list2.Add(item2);
        }

        return ([.. list1], [.. list2]);
    }
}

/// <summary>
/// Provides extension methods for safe dictionary operations.
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Safely retrieves a value from a dictionary.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="dictionary">The dictionary to search.</param>
    /// <param name="key">The key to find.</param>
    /// <returns>Some(value) if the key exists, otherwise None.</returns>
    public static Option<TValue> Find<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) =>
        dictionary.TryGetValue(key, out var value)
            ? Option.Some(value)
            : Option.None;
}