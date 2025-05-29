using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace common;

#pragma warning disable CA1716
public sealed record Option<T>
#pragma warning restore CA1716
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

    public bool IsNone => !isSome;
    public bool IsSome => isSome;

#pragma warning disable CA1000
    internal static Option<T> Some(T value) =>
        new(value);

    public static Option<T> None() =>
        new();
#pragma warning restore CA1000

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

    public T2 Match<T2>(Func<T, T2> some, Func<T2> none) =>
        IsSome ? some(value!) : none();

    public void Match(Action<T> some, Action none)
    {
        if (IsSome)
            some(value!);
        else
            none();
    }

    public static implicit operator Option<T>(T value) =>
        Some(value);

    public static implicit operator Option<T>(None _) =>
        None();
}

public sealed record None
{
    public static None Instance { get; } = new();

    private None() { }

    public override string ToString() =>
        "None";

    public bool Equals(None? other) =>
        other is not null;

    public override int GetHashCode() =>
        0;
}

#pragma warning disable CA1716
public static class Option
#pragma warning restore CA1716
{
    public static Option<T> Some<T>(T value) =>
        Option<T>.Some(value);

    public static None None =>
        None.Instance;

    public static Option<T> Where<T>(this Option<T> option, Func<T, bool> predicate) =>
        option.Match(t => predicate(t) ? option : None,
                     () => None);

    public static Option<T2> Map<T, T2>(this Option<T> option, Func<T, T2> f) =>
        option.Match(t => Some(f(t)),
                     () => None);

    public static Option<T2> Bind<T, T2>(this Option<T> option, Func<T, Option<T2>> f) =>
        option.Match(t => f(t),
                     () => None);

    public static Option<T2> Select<T, T2>(this Option<T> option, Func<T, T2> f) =>
        option.Map(f);

    public static Option<TResult> SelectMany<T, T2, TResult>(this Option<T> option, Func<T, Option<T2>> f,
                                                         Func<T, T2, TResult> selector) =>
        option.Bind(t => f(t).Map(t2 => selector(t, t2)));

    public static T IfNone<T>(this Option<T> option, Func<T> f) =>
        option.Match(t => t,
                     f);

    public static void Iter<T>(this Option<T> option, Action<T> f) =>
        option.Match(f,
                     () => { });

    public static T IfNoneThrow<T>(this Option<T> option, Exception exception) =>
        option.Match(t => t,
                     () => throw exception);

    public static async ValueTask IterTask<T>(this Option<T> option, Func<T, ValueTask> f) =>
        await option.Match<ValueTask>(async t => await f(t),
                                      () => ValueTask.CompletedTask);
}

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

    public bool IsSuccess => isSuccess;
    public bool IsError => isSuccess is false;

#pragma warning disable CA1000
    internal static Result<T> Success(T value) =>
        new(value);

    internal static Result<T> Error(Error error) =>
        new(error);
#pragma warning restore CA1000

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onError) =>
        IsSuccess ? onSuccess(value!) : onError(error!);

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

    public static implicit operator Result<T>(T value) =>
        Success(value);

    public static implicit operator Result<T>(Error error) =>
        Error(error);
}

public static class Result
{
    public static Result<T> Success<T>(T value) =>
        Result<T>.Success(value);

    public static Result<T> Error<T>(Error error) =>
        Result<T>.Error(error);

    public static Result<T2> Map<T, T2>(this Result<T> result, Func<T, T2> f) =>
        result.Match(value => Success(f(value)),
                     error => Error<T2>(error));

    public static Result<T> MapError<T>(this Result<T> result, Func<Error, Error> f) =>
        result.Match(value => Success(value),
                     error => Error<T>(f(error)));

    public static Result<T2> Bind<T, T2>(this Result<T> result, Func<T, Result<T2>> f) =>
        result.Match(value => f(value),
                     error => Error<T2>(error));

    public static Result<T2> Select<T, T2>(this Result<T> result, Func<T, T2> f) =>
        result.Map(f);

    public static Result<TResult> SelectMany<T, T2, TResult>(this Result<T> result, Func<T, Result<T2>> f,
                                                             Func<T, T2, TResult> selector) =>
        result.Bind(value => f(value)
              .Map(value2 => selector(value, value2)));

    public static T IfError<T>(this Result<T> result, Func<Error, T> f) =>
        result.Match(value => value,
                     f);

    public static void Iter<T>(this Result<T> result, Action<T> f) =>
        result.Match(f,
                     _ => { });

    public static async ValueTask IterTask<T>(this Result<T> result, Func<T, ValueTask> f) =>
        await result.Match<ValueTask>(async value => await f(value),
                                      _ => ValueTask.CompletedTask);

    public static T IfErrorThrow<T>(this Result<T> result) =>
        result.Match(value => value,
                     error => throw error.ToException());
}

#pragma warning disable CA1716 // Identifiers should not match keywords
public record Error
#pragma warning restore CA1716 // Identifiers should not match keywords
{
    private readonly ImmutableHashSet<string> messages;

    protected Error(IEnumerable<string> messages)
    {
        this.messages = [.. messages];
    }

    public ImmutableHashSet<string> Messages => messages;

    public static Error From(params string[] messages) =>
        new(messages);

    public static Error From(Exception exception) =>
        new Exceptional(exception);

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

    public static implicit operator Error(string message) =>
        From(message);

    public static implicit operator Error(Exception exception) =>
        From(exception);

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
            _ => messages.Aggregate(0, (hash, message) => hash ^ message.GetHashCode())
        };

    public sealed record Exceptional : Error
    {
        private readonly Exception exception;

        internal Exceptional(Exception exception) : base([exception.Message])
        {
            this.exception = exception;
        }

        public override Exception ToException() => exception;

        public bool Equals(Error.Exceptional? other) =>
            (this, other) switch
            {
                (_, null) => false,
                _ => exception.Equals(other.exception)
            };

        public override int GetHashCode() =>
            exception.GetHashCode();
    }
}

public static class EnumerableExtensions
{
    /// <summary>
    /// Returns an <see cref="Option{T}"/> containing the first element of the sequence, or <see cref="Option{T}.None"/> if the sequence is empty.
    /// </summary>
    public static Option<T> Head<T>(this IEnumerable<T> source)
    {
        using var enumerator = source.GetEnumerator();

        return enumerator.MoveNext()
                ? Option.Some(enumerator.Current)
                : Option.None;
    }

    /// <summary>
    /// Applies the function <paramref name="selector"/> to each element, then return values for which the function returns <see cref="Option{T2}.Some"/>.
    /// </summary>
    public static IEnumerable<T2> Choose<T, T2>(this IEnumerable<T> source, Func<T, Option<T2>> selector) =>
        source.Select(selector)
              .Where(option => option.IsSome)
              .Select(option => option.IfNone(() => throw new UnreachableException("All options should be in the 'Some' state.")));

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

    public static void Iter<T>(this IEnumerable<T> source, Action<T> action, Option<int> maxDegreeOfParallelism, CancellationToken cancellationToken)
    {
        var options = new ParallelOptions { CancellationToken = cancellationToken };
        maxDegreeOfParallelism.Iter(max => options.MaxDegreeOfParallelism = max);

        Parallel.ForEach(source, options, item => action(item));
    }

    public static async ValueTask IterTask<T>(this IEnumerable<T> source, Func<T, ValueTask> action, Option<int> maxDegreeOfParallelism, CancellationToken cancellationToken)
    {
        var options = new ParallelOptions { CancellationToken = cancellationToken };
        maxDegreeOfParallelism.Iter(max => options.MaxDegreeOfParallelism = max);

        await Parallel.ForEachAsync(source, options, async (item, _) => await action(item));
    }
}

public static class AsyncEnumerableExtensions
{
    /// <summary>
    /// Returns an <see cref="Option{T}"/> containing the first element of the sequence, or <see cref="Option{T}.None"/> if the sequence is empty.
    /// </summary>
    public static async ValueTask<Option<T>> Head<T>(this IAsyncEnumerable<T> source, CancellationToken cancellationToken)
    {
        await using var enumerator = source.GetAsyncEnumerator(cancellationToken);

        return await enumerator.MoveNextAsync()
                ? Option.Some(enumerator.Current)
                : Option.None;
    }

    /// <summary>
    /// Applies the function <paramref name="selector"/> to each element, then return values for which the function returns <see cref="Option{T2}.Some"/>.
    /// </summary>
    public static IAsyncEnumerable<T2> Choose<T, T2>(this IAsyncEnumerable<T> source, Func<T, Option<T2>> selector) =>
        source.Select(selector)
              .Where(option => option.IsSome)
              .Select(option => option.IfNone(() => throw new UnreachableException("All options should be in the 'Some' state.")));

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

    public static async ValueTask IterTask<T>(this IAsyncEnumerable<T> source, Func<T, ValueTask> action, Option<int> maxDegreeOfParallelism, CancellationToken cancellationToken)
    {
        var options = new ParallelOptions { CancellationToken = cancellationToken };
        maxDegreeOfParallelism.Iter(max => options.MaxDegreeOfParallelism = max);

        await Parallel.ForEachAsync(source, options, async (item, _) => await action(item));
    }
}

public static class DictionaryExtensions
{
    public static Option<TValue> Find<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) =>
        dictionary.TryGetValue(key, out var value)
            ? Option.Some(value)
            : Option.None;
}