using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using System;
using System.Text.Json;

namespace common;

public sealed record JsonError : Error, Semigroup<JsonError>
{
    private readonly JsonException exception;

    private JsonError(JsonException exception) => this.exception = exception;

    private JsonError(string message) : this(new JsonException(message)) { }

    public override string Message => exception.Message;

    public override bool IsExceptional { get; }

    public override bool IsExpected { get; } = true;

    public override Exception ToException() => exception;

    public override ErrorException ToErrorException() => ErrorException.New(exception);

    public static JsonError From(string message) => new(message);

    public static JsonError From(JsonException exception) => new(exception);

    public override bool HasException<T>() => typeof(T).IsAssignableFrom(typeof(JsonException));

    public JsonError Combine(JsonError rhs) =>
        new(new JsonException("Multiple errors, see inner exception for details.",
                              new AggregateException(exception.InnerException switch
                              {
                                  AggregateException aggregateException => [.. aggregateException.InnerExceptions, rhs.exception],
                                  _ => [exception, rhs.exception]
                              })));

    public static JsonError operator +(JsonError lhs, JsonError rhs) =>
        lhs.Combine(rhs);
}

public class JsonResult :
    Monad<JsonResult>,
    Traversable<JsonResult>,
    Choice<JsonResult>
{
    public static JsonResult<T> Succeed<T>(T value) =>
        JsonResult<T>.Succeed(value);

    public static JsonResult<T> Fail<T>(JsonError error) =>
        JsonResult<T>.Fail(error);

    public static JsonResult<T> Fail<T>(string errorMessage) =>
        JsonResult<T>.Fail(JsonError.From(errorMessage));

    public static K<JsonResult, T> Pure<T>(T value) =>
        Succeed(value);

    public static K<JsonResult, T2> Bind<T1, T2>(K<JsonResult, T1> ma, Func<T1, K<JsonResult, T2>> f) =>
        ma.As()
          .Match(f, Fail<T2>);

    public static K<JsonResult, T2> Map<T1, T2>(Func<T1, T2> f, K<JsonResult, T1> ma) =>
        ma.As()
          .Match(t1 => Pure(f(t1)),
                 Fail<T2>);

    public static K<JsonResult, T2> Apply<T1, T2>(K<JsonResult, Func<T1, T2>> mf, K<JsonResult, T1> ma) =>
        mf.As()
          .Match(f => ma.Map(f),
                 error1 => ma.As()
                             .Match(t1 => Fail<T2>(error1),
                                    error2 => Fail<T2>(error1 + error2)));

    public static K<JsonResult, T> Choose<T>(K<JsonResult, T> fa, K<JsonResult, T> fb) =>
        fa.As()
          .Match(_ => fa,
                 _ => fb);

    public static K<TApplicative, K<JsonResult, T2>> Traverse<TApplicative, T1, T2>(Func<T1, K<TApplicative, T2>> f, K<JsonResult, T1> ta) where TApplicative : Applicative<TApplicative> =>
        (K<TApplicative, K<JsonResult, T2>>)
        ta.As()
          .Match(t1 => f(t1).Map(Succeed),
                 error => TApplicative.Pure(Fail<T2>(error)));

    public static S FoldWhile<A, S>(Func<A, Func<S, S>> f, Func<(S State, A Value), bool> predicate, S initialState, K<JsonResult, A> ta) =>
        ta.As()
          .Match(a => predicate((initialState, a))
                          ? f(a)(initialState)
                          : initialState,
                 _ => initialState);

    public static S FoldBackWhile<A, S>(Func<S, Func<A, S>> f, Func<(S State, A Value), bool> predicate, S initialState, K<JsonResult, A> ta) =>
        ta.As()
          .Match(a => predicate((initialState, a))
                          ? f(initialState)(a)
                          : initialState,
                 _ => initialState);
}

public class JsonResult<T> :
    IEquatable<JsonResult<T>>,
    K<JsonResult, T>
{
    private readonly Either<JsonError, T> value;

    private JsonResult(Either<JsonError, T> value) => this.value = value;

    public T2 Match<T2>(Func<T, T2> Succ, Func<JsonError, T2> Fail) =>
        value.Match(Fail, Succ);

    public Unit Match(Action<T> Succ, Action<JsonError> Fail) =>
        value.Match(Fail, Succ);

    internal static JsonResult<T> Succeed(T value) =>
        new(value);

    internal static JsonResult<T> Fail(JsonError error) =>
        new(error);

    public override bool Equals(object? obj) =>
        obj is JsonResult<T> result && Equals(result);

    public override int GetHashCode() =>
        value.GetHashCode();

    public bool Equals(JsonResult<T>? other) =>
        other is not null
        && this.Match(t => other.Match(t2 => t?.Equals(t2) ?? false,
                                       _ => false),
                      error => other.Match(_ => false,
                                           error2 => error.Equals(error2)));
}

public static class JsonResultExtensions
{
    public static JsonResult<T> As<T>(this K<JsonResult, T> k) =>
        (JsonResult<T>)k;

    public static JsonResult<T2> Map<T1, T2>(this JsonResult<T1> result, Func<T1, T2> f) =>
        result.Match(t1 => JsonResult.Succeed(f(t1)),
                     JsonResult<T2>.Fail);

    public static JsonResult<T2> Bind<T1, T2>(this JsonResult<T1> result, Func<T1, JsonResult<T2>> f) =>
        result.Match(f, JsonResult<T2>.Fail);

    public static JsonResult<T> ReplaceError<T>(this JsonResult<T> result, string newErrorMessage) =>
        result.ReplaceError(JsonError.From(newErrorMessage));

    public static JsonResult<T> ReplaceError<T>(this JsonResult<T> result, JsonError newError) =>
        result.ReplaceError(_ => newError);

    public static JsonResult<T> ReplaceError<T>(this JsonResult<T> result, Func<JsonError, JsonError> f) =>
        result.Match(_ => result,
                     error => JsonResult.Fail<T>(f(error)));

    public static T IfError<T>(this JsonResult<T> result, Func<JsonError, T> f) =>
        result.Match(t => t, f);

    public static T ThrowIfFail<T>(this JsonResult<T> result) =>
        result.IfError(error => throw error.ToException());
}