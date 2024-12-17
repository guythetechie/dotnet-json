using LanguageExt;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace common;

public static class JsonArrayModule
{
    public static JsonArray ToJsonArray(this IEnumerable<JsonNode?> nodes) =>
        new([.. nodes]);

    public static ValueTask<JsonArray> ToJsonArray(this IAsyncEnumerable<JsonNode?> nodes, CancellationToken cancellationToken) =>
        nodes.AggregateAsync(new JsonArray(),
                            (array, node) =>
                            {
                                array.Add(node);
                                return array;
                            },
                            cancellationToken);

    public static JsonResult<ImmutableArray<JsonObject>> GetJsonObjects(this JsonArray jsonArray) =>
        jsonArray.GetElements(jsonNode => jsonNode.AsJsonObject(),
                              index => JsonError.From($"Node at index {index} is not a JSON object."));

    public static JsonResult<ImmutableArray<T>> GetElements<T>(this JsonArray jsonArray, Func<JsonNode?, JsonResult<T>> selector, Func<int, JsonError> errorFromIndex)
    {
        return jsonArray.Select((node, index) => (node, index))
                        .AsIterable()
                        .Traverse(x => nodeToElement(x.node, x.index))
                        .Map(iterable => iterable.ToImmutableArray())
                        .As();

        JsonResult<T> nodeToElement(JsonNode? node, int index) =>
            selector(node)
                .ReplaceError(_ => errorFromIndex(index));
    }

    public static JsonResult<ImmutableArray<JsonArray>> GetJsonArrays(this JsonArray jsonArray) =>
        jsonArray.GetElements(jsonNode => jsonNode.AsJsonArray(),
                              index => JsonError.From($"Node at index {index} is not a JSON array."));

    public static JsonResult<ImmutableArray<JsonValue>> GetJsonValues(this JsonArray jsonArray) =>
        jsonArray.GetElements(jsonNode => jsonNode.AsJsonValue(),
                              index => JsonError.From($"Node at index {index} is not a JSON value."));
}