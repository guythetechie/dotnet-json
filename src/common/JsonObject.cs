using LanguageExt;
using System;
using System.Text.Json.Nodes;

namespace common;

public static class JsonObjectModule
{
    public static JsonResult<JsonNode> GetProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject switch
        {
            null => JsonResult.Fail<JsonNode>("JSON object is null."),
            _ => jsonObject.TryGetPropertyValue(propertyName, out var jsonNode)
                    ? jsonNode switch
                    {
                        null => JsonResult.Fail<JsonNode>($"Property '{propertyName}' is null."),
                        _ => JsonResult.Succeed(jsonNode)
                    }
                    : JsonResult.Fail<JsonNode>($"JSON object does not have a property named '{propertyName}'.")
        };

    public static Option<JsonNode> GetOptionalProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.GetProperty(propertyName)
                  .Match(Option<JsonNode>.Some, _ => Option<JsonNode>.None);

    public static JsonResult<T> GetProperty<T>(this JsonObject? jsonObject, string propertyName, Func<JsonNode, JsonResult<T>> selector) =>
        jsonObject.GetProperty(propertyName)
                  .Bind(selector)
                  .AddPropertyNameToErrorMessage(propertyName);

    public static JsonResult<JsonObject> GetJsonObjectProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.GetProperty(propertyName, jsonNode => jsonNode.AsJsonObject());

    private static JsonResult<T> AddPropertyNameToErrorMessage<T>(this JsonResult<T> result, string propertyName)
    {
        return result.ReplaceError(replaceError);

        JsonError replaceError(JsonError error) =>
            JsonError.From($"Property '{propertyName}' is invalid. {error.Message}");
    }

    public static JsonResult<JsonArray> GetJsonArrayProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.GetProperty(propertyName, jsonNode => jsonNode.AsJsonArray());

    public static JsonResult<JsonValue> GetJsonValueProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.GetProperty(propertyName, jsonNode => jsonNode.AsJsonValue());

    public static JsonResult<string> GetStringProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.GetProperty(propertyName, jsonNode => jsonNode.AsJsonValue()
                                                                 .Bind(jsonValue => jsonValue.AsString()));

    public static JsonResult<int> GetIntProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.GetProperty(propertyName, jsonNode => jsonNode.AsJsonValue()
                                                                 .Bind(jsonValue => jsonValue.AsInt()));

    public static JsonResult<bool> GetBoolProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.GetProperty(propertyName, jsonNode => jsonNode.AsJsonValue()
                                                                 .Bind(jsonValue => jsonValue.AsBool()));

    public static JsonResult<Guid> GetGuidProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.GetProperty(propertyName, jsonNode => jsonNode.AsJsonValue()
                                                                 .Bind(jsonValue => jsonValue.AsGuid()));

    public static JsonResult<Uri> GetAbsoluteUriProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.GetProperty(propertyName, jsonNode => jsonNode.AsJsonValue()
                                                                 .Bind(jsonValue => jsonValue.AsAbsoluteUri()));

    public static JsonObject SetProperty(this JsonObject jsonObject, string propertyName, JsonNode? propertyValue)
    {
        jsonObject[propertyName] = propertyValue;
        return jsonObject;
    }
}