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

    public static Result<ImmutableArray<JsonObject>> GetJsonObjects(this JsonArray jsonArray) =>
        jsonArray.GetElements(jsonNode => jsonNode.AsJsonObject(),
                              index => Error.From($"Node at index {index} is not a JSON object."));

    public static Result<ImmutableArray<T>> GetElements<T>(this JsonArray jsonArray, Func<JsonNode?, Result<T>> selector, Func<int, Error> errorFromIndex)
    {
        return jsonArray.Select((node, index) => (node, index))
                        .Traverse(x => nodeToElement(x.node, x.index), CancellationToken.None);

        Result<T> nodeToElement(JsonNode? node, int index) =>
            selector(node)
                .MapError(error => errorFromIndex(index));
    }

    public static Result<ImmutableArray<JsonArray>> GetJsonArrays(this JsonArray jsonArray) =>
        jsonArray.GetElements(jsonNode => jsonNode.AsJsonArray(),
                              index => Error.From($"Node at index {index} is not a JSON array."));

    public static Result<ImmutableArray<JsonValue>> GetJsonValues(this JsonArray jsonArray) =>
        jsonArray.GetElements(jsonNode => jsonNode.AsJsonValue(),
                              index => Error.From($"Node at index {index} is not a JSON value."));
}