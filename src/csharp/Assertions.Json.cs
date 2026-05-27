using System;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using TUnit.Assertions.Core;
using TUnit.Assertions.Sources;

namespace common;

public class JsonNodeIsEquivalentToJsonNodeAssertion<T>(AssertionContext<T> context, JsonNode? expected)
    : Assertion<T>(context) where T : JsonNode
{
    protected override string GetExpectation() =>
        $"JSON node to be equivalent to {expected?.ToJsonString() ?? "null"}";

    protected override async Task<AssertionResult> CheckAsync(EvaluationMetadata<T> metadata)
    {
        await ValueTask.CompletedTask;

        return (metadata.Exception, metadata.Value, expected) switch
        {
            (not null, _, _) => AssertionResult.Failed($"threw {metadata.Exception.GetType().Name} with message {metadata.Exception.Message}", metadata.Exception),
            (_, null, null) => AssertionResult.Passed,
            (_, null, not null) => AssertionResult.Failed("was null"),
            (_, var actual, var expected) when actual.ToJsonString() == expected?.ToJsonString() => AssertionResult.Passed,
            (_, var actual, _) => AssertionResult.Failed($"was {actual.ToJsonString()}")
        };
    }
}

public class JsonObjectHasStringPropertyAssertion(AssertionContext<JsonObject> context, string propertyName)
    : MappedValueAssertion<JsonObject, string>(context,
                                               jsonObject => ValueTask.FromResult(jsonObject.GetStringProperty(propertyName)))
{
    protected override string GetExpectation() => $"JSON object to have string property '{propertyName}'";
}

public class JsonObjectHasIntPropertyAssertion(AssertionContext<JsonObject> context, string propertyName)
    : MappedValueAssertion<JsonObject, int>(context,
                                            jsonObject => ValueTask.FromResult(jsonObject.GetIntProperty(propertyName)))
{
    public ValueAssertion<int> Which =>
        GetMappedAssertion();

    protected override string GetExpectation() => $"JSON object to have int property '{propertyName}'";
}

public class JsonObjectHasBoolPropertyAssertion(AssertionContext<JsonObject> context, string propertyName)
    : MappedValueAssertion<JsonObject, bool>(context,
                                             jsonObject => ValueTask.FromResult(jsonObject.GetBoolProperty(propertyName)))
{
    public ValueAssertion<bool> Which =>
        GetMappedAssertion();

    protected override string GetExpectation() => $"JSON object to have bool property '{propertyName}'";
}

public class JsonObjectHasJsonArrayPropertyAssertion(AssertionContext<JsonObject> context, string propertyName)
    : MappedValueAssertion<JsonObject, JsonArray>(context,
                                                  jsonObject => ValueTask.FromResult(jsonObject.GetJsonArrayProperty(propertyName)))
{
    public ValueAssertion<JsonArray> Which =>
        GetMappedAssertion();

    protected override string GetExpectation() => $"JSON object to have JSON array property '{propertyName}'";
}

public class JsonObjectCanBeDeserializedTo<T>(AssertionContext<JsonObject> context, Func<JsonObject, Result<T>> deserialize)
    : MappedValueAssertion<JsonObject, T>(context,
                                          jsonObject => ValueTask.FromResult(deserialize(jsonObject)))
{
    public ValueAssertion<T> Which =>
        GetMappedAssertion();

    protected override string GetExpectation() => $"JSON object to be deserializable to '{typeof(T).Name}'";
}



public static class JsonNodeAssertionExtensions
{
    extension(IAssertionSource<JsonObject> source)
    {
        public JsonObjectHasStringPropertyAssertion HasStringProperty(string propertyName) =>
            new(source.Context, propertyName);

        public JsonObjectHasIntPropertyAssertion HasIntProperty(string propertyName) =>
            new(source.Context, propertyName);

        public JsonObjectHasBoolPropertyAssertion HasBoolProperty(string propertyName) =>
            new(source.Context, propertyName);

        public JsonObjectHasJsonArrayPropertyAssertion HasJsonArrayProperty(string propertyName) =>
            new(source.Context, propertyName);

        public JsonObjectCanBeDeserializedTo<T> CanBeDeserializedTo<T>(Func<JsonObject, Result<T>> deserialize) =>
            new(source.Context, deserialize);
    }

    extension<T>(IAssertionSource<T> source) where T : JsonNode
    {
        public JsonNodeIsEquivalentToJsonNodeAssertion<T> IsEquivalentTo(JsonNode? expected, [CallerArgumentExpression(nameof(expected))] string? expectedExpression = null)
        {
            var context = source.Context;

            context.ExpressionBuilder.Append(".IsEquivalentTo(");
            context.ExpressionBuilder.Append(expectedExpression);
            context.ExpressionBuilder.Append(')');

            return new(context, expected);
        }
    }
}