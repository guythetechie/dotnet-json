using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace common;

/// <summary>
/// Provides extension methods for working with <see cref="JsonArray"/> instances in a functional style.
/// </summary>
public static class JsonArrayModule
{
    /// <summary>
    /// Converts an enumerable of <see cref="JsonNode"/> instances to a <see cref="JsonArray"/>.
    /// </summary>
    /// <param name="nodes">The enumerable to convert.</param>
    /// <returns>A <see cref="JsonArray"/> containing all nodes.</returns>
    public static JsonArray ToJsonArray(this IEnumerable<JsonNode?> nodes) =>
        new([.. nodes]);

    /// <summary>
    /// Asynchronously converts an async enumerable of <see cref="JsonNode"/> instances to a <see cref="JsonArray"/>.
    /// </summary>
    /// <param name="nodes">The async enumerable to convert.</param>
    /// <param name="cancellationToken">Cancellation token to observe during the operation.</param>
    /// <returns>A task that yields a <see cref="JsonArray"/> containing all nodes.</returns>
    public static ValueTask<JsonArray> ToJsonArray(this IAsyncEnumerable<JsonNode?> nodes, CancellationToken cancellationToken) =>
        nodes.AggregateAsync(new JsonArray(),
                            (array, node) =>
                            {
                                array.Add(node);
                                return array;
                            },
                            cancellationToken);

    /// <summary>
    /// Extracts all <see cref="JsonObject"/> elements from a <see cref="JsonArray"/>.
    /// </summary>
    /// <param name="jsonArray">The <see cref="JsonArray"/> to process.</param>
    /// <returns>Success with <see cref="JsonObject"/>s if all elements are objects, otherwise error details.</returns>
    public static Result<ImmutableArray<JsonObject>> GetJsonObjects(this JsonArray jsonArray) =>
        jsonArray.GetElements(jsonNode => jsonNode.AsJsonObject(),
                              index => Error.From($"Node at index {index} is not a JSON object."));

    /// <summary>
    /// Extracts elements from a <see cref="JsonArray"/> using a selector function, collecting successes or aggregating errors.
    /// </summary>
    /// <typeparam name="T">The type of elements to extract.</typeparam>
    /// <param name="jsonArray">The <see cref="JsonArray"/> to process.</param>
    /// <param name="selector">Function that converts a <see cref="JsonNode"/> to the desired type.</param>
    /// <param name="errorFromIndex">Function that creates an error message for a failed conversion at a given index.</param>
    /// <returns>Success with extracted elements if all conversions succeed, otherwise aggregated error details.</returns>
    public static Result<ImmutableArray<T>> GetElements<T>(this JsonArray jsonArray, Func<JsonNode?, Result<T>> selector, Func<int, Error> errorFromIndex)
    {
        return jsonArray.Select((node, index) => (node, index))
                        .Traverse(x => nodeToElement(x.node, x.index), CancellationToken.None);

        Result<T> nodeToElement(JsonNode? node, int index) =>
            selector(node)
                .MapError(error => errorFromIndex(index));
    }

    /// <summary>
    /// Extracts all <see cref="JsonArray"/> elements from a <see cref="JsonArray"/>.
    /// </summary>
    /// <param name="jsonArray">The <see cref="JsonArray"/> to process.</param>
    /// <returns>Success with <see cref="JsonArray"/>s if all elements are arrays, otherwise error details.</returns>
    public static Result<ImmutableArray<JsonArray>> GetJsonArrays(this JsonArray jsonArray) =>
        jsonArray.GetElements(jsonNode => jsonNode.AsJsonArray(),
                              index => Error.From($"Node at index {index} is not a JSON array."));

    /// <summary>
    /// Extracts all <see cref="JsonValue"/> elements from a <see cref="JsonArray"/>.
    /// </summary>
    /// <param name="jsonArray">The <see cref="JsonArray"/> to process.</param>
    /// <returns>Success with <see cref="JsonValue"/>s if all elements are values, otherwise error details.</returns>
    public static Result<ImmutableArray<JsonValue>> GetJsonValues(this JsonArray jsonArray) =>
        jsonArray.GetElements(jsonNode => jsonNode.AsJsonValue(),
                              index => Error.From($"Node at index {index} is not a JSON value."));
}