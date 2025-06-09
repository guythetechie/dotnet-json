using common;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace common.tests;

internal sealed class OptionAssertions<T>(Option<T> subject, AssertionChain assertionChain) : ReferenceTypeAssertions<Option<T>, OptionAssertions<T>>(subject, assertionChain)
{
    protected override string Identifier { get; } = "option";

    public AndWhichConstraint<OptionAssertions<T>, T> BeSome([StringSyntax("CompositeFormat")] string because = "", params object[] becauseArgs)
    {
        CurrentAssertionChain
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject.IsSome)
            .FailWith("Expected {context:option} to be some{reason}, but it was none.");

        return new AndWhichConstraint<OptionAssertions<T>, T>(
            this,
            Subject.IfNoneThrow(() => new UnreachableException()));
    }

    public AndConstraint<OptionAssertions<T>> BeNone([StringSyntax("CompositeFormat")] string because = "", params object[] becauseArgs)
    {
        CurrentAssertionChain
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject.IsNone)
            .FailWith("Expected {context:option} to be none{reason}, but it was some with value {0}.",
                       () => Subject.Match(value => value, () => throw new UnreachableException()));

        return new AndConstraint<OptionAssertions<T>>(this);
    }

    public AndConstraint<OptionAssertions<T>> Be(Option<T> expected, [StringSyntax("CompositeFormat")] string because = "", params object[] becauseArgs)
    {
        CurrentAssertionChain
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject.Equals(expected))
            .FailWith("Expected {context:option} to be {0}{reason}, but it was {1}.", expected, Subject);

        return new AndConstraint<OptionAssertions<T>>(this);
    }
}

internal sealed class ResultAssertions<T>(Result<T> subject, AssertionChain assertionChain) : ReferenceTypeAssertions<Result<T>, ResultAssertions<T>>(subject, assertionChain)
{
    protected override string Identifier { get; } = "result";

    public AndWhichConstraint<ResultAssertions<T>, T> BeSuccess([StringSyntax("CompositeFormat")] string because = "", params object[] becauseArgs)
    {
        CurrentAssertionChain
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject.IsSuccess)
            .FailWith("Expected {context:result} to be success{reason}, but it was error with error {0}.",
                       () => Subject.Match(_ => throw new UnreachableException(), error => error));

        return new AndWhichConstraint<ResultAssertions<T>, T>(
            this,
            Subject.IfErrorThrow());
    }

    public AndWhichConstraint<ResultAssertions<T>, Error> BeError([StringSyntax("CompositeFormat")] string because = "", params object[] becauseArgs)
    {
        CurrentAssertionChain
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject.IsError)
            .FailWith("Expected {context:result} to be error{reason}, but it was success with value {0}.",
                       () => Subject.IfErrorThrow());

        return new AndWhichConstraint<ResultAssertions<T>, Error>(
            this,
            Subject.Match(success => throw (new UnreachableException()), error => error));
    }

    public AndConstraint<ResultAssertions<T>> Be(Result<T> expected, [StringSyntax("CompositeFormat")] string because = "", params object[] becauseArgs)
    {
        CurrentAssertionChain
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject.Equals(expected))
            .FailWith("Expected {context:result} to be {0}{reason}, but it was {1}.", expected, Subject);

        return new AndConstraint<ResultAssertions<T>>(this);
    }
}

internal class JsonNodeAssertions(JsonNode instance, AssertionChain assertionChain) : ReferenceTypeAssertions<JsonNode, JsonNodeAssertions>(instance, assertionChain)
{
    private readonly AssertionChain assertionChain = assertionChain;

    protected override string Identifier { get; } = "node";

    public AndConstraint<JsonNode> BeEquivalentTo(JsonNode? expected, JsonSerializerOptions? options = null, string because = "", params object[] becauseArgs)
    {
        var optionsToUse = options ?? new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        var actualString = Subject.ToJsonString(optionsToUse);
        var expectedString = expected?.ToJsonString(optionsToUse);

        assertionChain.BecauseOf(because, becauseArgs)
                      .ForCondition(actualString.Equals(expectedString, StringComparison.OrdinalIgnoreCase))
                      .FailWith("Expected {context:JSON node} to be equivalent to {0}{reason}, but found {1}.", expectedString, actualString);

        return new(Subject);
    }
}

internal sealed class JsonObjectAssertions(JsonObject instance, AssertionChain assertionChain) : JsonNodeAssertions(instance, assertionChain)
{
    private readonly AssertionChain assertionChain = assertionChain;
    protected override string Identifier { get; } = "JSON object";

    public AndWhichConstraint<JsonObjectAssertions, JsonNode?> ContainProperty(string propertyName, [StringSyntax("CompositeFormat")] string because = "", params object[] becauseArgs)
    {
        assertionChain
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject.AsObject().ContainsKey(propertyName))
            .FailWith("Expected {context:JSON object} to contain property '{0}'{reason}, but it did not.", propertyName);

        return new AndWhichConstraint<JsonObjectAssertions, JsonNode?>(this, Subject[propertyName]);
    }

    public AndConstraint<JsonObjectAssertions> NotContainProperty(string propertyName, [StringSyntax("CompositeFormat")] string because = "", params object[] becauseArgs)
    {
        assertionChain
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject.AsObject().ContainsKey(propertyName) is false)
            .FailWith("Expected {context:JSON object} not to contain property '{0}'{reason}, but it did.", propertyName);

        return new AndConstraint<JsonObjectAssertions>(this);
    }
}

internal static class AssertionExtensions
{
    public static JsonNodeAssertions Should(this JsonNode subject) =>
        new(subject, AssertionChain.GetOrCreate());

    public static JsonObjectAssertions Should(this JsonObject subject) =>
        new(subject, AssertionChain.GetOrCreate());

    public static OptionAssertions<T> Should<T>(this Option<T> subject) =>
        new(subject, AssertionChain.GetOrCreate());

    public static ResultAssertions<T> Should<T>(this Result<T> subject) =>
        new(subject, AssertionChain.GetOrCreate());
}