using CsCheck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace common;

#pragma warning disable CA1720 // Identifier contains type name
public static class JsonValueGenerator
{
    public static Gen<JsonValue> String { get; } =
        from value in Gen.String
        select JsonValue.Create(value);

    public static Gen<JsonValue> Int { get; } =
        from value in Gen.Int
        select JsonValue.Create(value);

    public static Gen<JsonValue> Bool { get; } =
        from value in Gen.OneOfConst(true, false)
        select JsonValue.Create(value);

    public static Gen<JsonValue> Guid { get; } =
        from value in Gen.Guid
        select JsonValue.Create(value);

    public static Gen<JsonValue> AbsoluteUri { get; } =
        from value in Gen.String.AlphaNumeric
        where string.IsNullOrWhiteSpace(value) is false
        let uri = new Uri($"https://{value}.com", UriKind.Absolute)
        select JsonValue.Create(uri);

    public static Gen<JsonValue> Any { get; } =
        Gen.OneOf(String,
                  Int,
                  Bool,
                  Guid,
                  AbsoluteUri);

    public static Gen<JsonValue> WhereKind(this Gen<JsonValue> gen, Func<JsonValueKind, bool> predicate) =>
        gen.Where(value => predicate(value.GetValueKind()));

    private static Gen<JsonValue> WhereObject(this Gen<JsonValue> gen, Func<object, bool> predicate) =>
        gen.Where(value => predicate(value.GetValue<object>()));

    public static Gen<JsonValue> WhereObjectString(this Gen<JsonValue> gen, Func<string?, bool> predicate) =>
        gen.WhereObject(value => predicate(value.ToString()));
}

public static class JsonNodeGenerator
{
    public static Gen<JsonNode> Any { get; } = Create();

    public static Gen<JsonNode> JsonArray { get; } =
        from node in JsonArrayGenerator.Create(Any)
        select node as JsonNode;

    public static Gen<JsonNode> JsonValue { get; } =
        from node in JsonValueGenerator.Any
        select node as JsonNode;

    public static Gen<JsonNode> JsonObject { get; } =
        from node in JsonObjectGenerator.Create(Any)
        select node as JsonNode;

    public static Gen<JsonNode> Create() =>
        Gen.Recursive<JsonNode>((depth, nodeGen) => depth < 2
                                                    ? Gen.OneOf<JsonNode>(JsonObjectGenerator.Create(nodeGen),
                                                                          JsonArrayGenerator.Create(nodeGen),
                                                                          JsonValueGenerator.Any)
                                                    : from jsonValue in JsonValueGenerator.Any
                                                      select jsonValue as JsonNode);
}

public static class JsonArrayGenerator
{
    public static Gen<JsonArray> Any { get; } = Create(JsonNodeGenerator.Any);

    public static Gen<JsonArray> Create(Gen<JsonNode> nodeGen) =>
        from elements in nodeGen.Null().Array
        select new JsonArray(elements);
}

public static class JsonObjectGenerator
{
    public static Gen<JsonObject> Any { get; } = Create(JsonNodeGenerator.Any);

    public static Gen<JsonObject> Create(Gen<JsonNode> nodeGen) =>
        from elements in Gen.Select(Gen.String.AlphaNumeric, nodeGen.Null()).List[0, 10]
        let kvps = elements.Select(element => KeyValuePair.Create(element.Item1, element.Item2))
                           .Where(kvp => string.IsNullOrWhiteSpace(kvp.Key) is false)
                           .DistinctBy(kvp => kvp.Key.ToUpperInvariant())
        select new JsonObject(kvps);
}
#pragma warning restore CA1720 // Identifier contains type name