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
}