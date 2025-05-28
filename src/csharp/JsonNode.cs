using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace common;

public static class JsonNodeModule
{
    public static Result<JsonObject> AsJsonObject(this JsonNode? node) =>
        node switch
        {
            JsonObject jsonObject => Result.Success(jsonObject),
            null => Error.From("JSON node is null."),
            _ => Error.From("JSON node is not a JSON object.")
        };

    public static Result<JsonArray> AsJsonArray(this JsonNode? node) =>
        node switch
        {
            JsonArray jsonArray => Result.Success(jsonArray),
            null => Error.From("JSON node is null."),
            _ => Error.From("JSON node is not a JSON array.")
        };

    public static Result<JsonValue> AsJsonValue(this JsonNode? node) =>
        node switch
        {
            JsonValue jsonValue => Result.Success(jsonValue),
            null => Error.From("JSON node is null."),
            _ => Error.From("JSON node is not a JSON value.")
        };

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

    public static Stream ToStream(JsonNode node, JsonSerializerOptions? options = default) =>
        BinaryData.FromObjectAsJson(node, options ?? JsonSerializerOptions.Web)
                  .ToStream();
}