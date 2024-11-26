using System.Text.Json;
using System;
using System.Text.Json.Nodes;

namespace common;

public static class JsonNodeModule
{
    public static JsonResult<JsonObject> AsJsonObject(this JsonNode? node) =>
        node switch
        {
            JsonObject jsonObject => JsonResult.Succeed(jsonObject),
            null => JsonResult.Fail<JsonObject>("JSON node is null."),
            _ => JsonResult.Fail<JsonObject>("JSON node is not a JSON object.")
        };

    public static JsonResult<JsonArray> AsJsonArray(this JsonNode? node) =>
        node switch
        {
            JsonArray jsonArray => JsonResult.Succeed(jsonArray),
            null => JsonResult.Fail<JsonArray>("JSON node is null."),
            _ => JsonResult.Fail<JsonArray>("JSON node is not a JSON array.")
        };

    public static JsonResult<JsonValue> AsJsonValue(this JsonNode? node) =>
        node switch
        {
            JsonValue jsonValue => JsonResult.Succeed(jsonValue),
            null => JsonResult.Fail<JsonValue>("JSON node is null."),
            _ => JsonResult.Fail<JsonValue>("JSON node is not a JSON value.")
        };

    public static JsonResult<T> Deserialize<T>(BinaryData? data, JsonSerializerOptions? options = default)
    {
        if (data is null)
        {
            return JsonResult.Fail<T>("Binary data is null.");
        }

        try
        {
            var jsonObject = JsonSerializer.Deserialize<T>(data, options ?? JsonSerializerOptions.Web);

            return jsonObject is null
                ? JsonResult.Fail<T>("Deserialization return a null result.")
                : JsonResult.Succeed(jsonObject);
        }
        catch (JsonException exception)
        {
            var jsonError = JsonError.From(exception);
            return JsonResult.Fail<T>(jsonError);
        }
    }
}