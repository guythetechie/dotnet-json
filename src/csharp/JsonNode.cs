using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace common;

/// <summary>
/// Provides extension methods for working with <see cref="JsonNode"/> instances in a functional style.
/// </summary>
public static class JsonNodeModule
{
    /// <summary>
    /// Safely casts a <see cref="JsonNode"/> to a <see cref="JsonObject"/>.
    /// </summary>
    /// <param name="node">The <see cref="JsonNode"/> to cast.</param>
    /// <returns>Success with the <see cref="JsonObject"/> if cast succeeds, otherwise error details.</returns>
    public static Result<JsonObject> AsJsonObject(this JsonNode? node) =>
        node switch
        {
            JsonObject jsonObject => Result.Success(jsonObject),
            null => Error.From("JSON node is null."),
            _ => Error.From("JSON node is not a JSON object.")
        };

    /// <summary>
    /// Safely casts a <see cref="JsonNode"/> to a <see cref="JsonArray"/>.
    /// </summary>
    /// <param name="node">The <see cref="JsonNode"/> to cast.</param>
    /// <returns>Success with the <see cref="JsonArray"/> if cast succeeds, otherwise error details.</returns>
    public static Result<JsonArray> AsJsonArray(this JsonNode? node) =>
        node switch
        {
            JsonArray jsonArray => Result.Success(jsonArray),
            null => Error.From("JSON node is null."),
            _ => Error.From("JSON node is not a JSON array.")
        };

    /// <summary>
    /// Safely casts a <see cref="JsonNode"/> to a <see cref="JsonValue"/>.
    /// </summary>
    /// <param name="node">The <see cref="JsonNode"/> to cast.</param>
    /// <returns>Success with the <see cref="JsonValue"/> if cast succeeds, otherwise error details.</returns>
    public static Result<JsonValue> AsJsonValue(this JsonNode? node) =>
        node switch
        {
            JsonValue jsonValue => Result.Success(jsonValue),
            null => Error.From("JSON node is null."),
            _ => Error.From("JSON node is not a JSON value.")
        };

    /// <summary>
    /// Asynchronously parses JSON data from a stream into a <see cref="JsonNode"/>.
    /// </summary>
    /// <param name="data">The <see cref="Stream"/> containing JSON data.</param>
    /// <param name="nodeOptions">Options to control parsing behavior.</param>
    /// <param name="documentOptions">Options to control document parsing.</param>
    /// <param name="cancellationToken">Cancellation token to observe during the operation.</param>
    /// <returns>A task that yields Success with the parsed <see cref="JsonNode"/>, or error if parsing fails.</returns>
    public static async ValueTask<Result<JsonNode>> From(Stream? data,
                                                         JsonNodeOptions? nodeOptions = default,
                                                         JsonDocumentOptions documentOptions = default,
                                                         CancellationToken cancellationToken = default)
    {
        try
        {
            return data switch
            {
                null => Result.Error<JsonNode>("Binary data is null."),
                _ => await JsonNode.ParseAsync(data, nodeOptions, documentOptions, cancellationToken) switch
                {
                    null => Error.From("Deserialization returned a null result."),
                    var node => node
                }
            };
        }
        catch (JsonException exception)
        {
            return Error.From(exception);
        }
    }

    /// <summary>
    /// Parses JSON data from binary data into a <see cref="JsonNode"/>.
    /// </summary>
    /// <param name="data">The <see cref="BinaryData"/> containing JSON.</param>
    /// <param name="options">Options to control parsing behavior.</param>
    /// <returns>Success with the parsed <see cref="JsonNode"/>, or error if parsing fails.</returns>
    public static Result<JsonNode> From(BinaryData? data, JsonNodeOptions? options = default)
    {
        try
        {
            return data switch
            {
                null => Result.Error<JsonNode>("Binary data is null."),
                _ => JsonNode.Parse(data, options) switch
                {
                    null => Error.From("Deserialization returned a null result."),
                    var node => node
                }
            };
        }
        catch (JsonException exception)
        {
            return Error.From(exception);
        }
    }

    /// <summary>
    /// Deserializes JSON binary data into a strongly-typed object.
    /// </summary>
    /// <typeparam name="T">The type to deserialize into.</typeparam>
    /// <param name="data">The <see cref="BinaryData"/> containing JSON.</param>
    /// <param name="options">Serializer options. Defaults to <see cref="JsonSerializerOptions.Web"/> if null.</param>
    /// <returns>Success with the deserialized object, or error if deserialization fails.</returns>
    public static Result<T> Deserialize<T>(BinaryData? data, JsonSerializerOptions? options = default)
    {
        if (data is null)
        {
            return Error.From("Binary data is null.");
        }

        try
        {
            var jsonObject = JsonSerializer.Deserialize<T>(data, options ?? JsonSerializerOptions.Web);

            return jsonObject is null
                ? Error.From("Deserialization return a null result.")
                : jsonObject;
        }
        catch (JsonException exception)
        {
            return Error.From(exception);
        }
    }

    /// <summary>
    /// Converts a <see cref="JsonNode"/> to a stream containing the serialized JSON data.
    /// </summary>
    /// <param name="node">The <see cref="JsonNode"/> to serialize.</param>
    /// <param name="options">Serializer options. Defaults to <see cref="JsonSerializerOptions.Web"/> if null.</param>
    /// <returns>A <see cref="Stream"/> containing the serialized JSON.</returns>
    public static Stream ToStream(JsonNode node, JsonSerializerOptions? options = default) =>
        BinaryData.FromObjectAsJson(node, options ?? JsonSerializerOptions.Web)
                  .ToStream();
}