using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace common;

public static class JsonObjectModule
{
    public static Result<JsonNode> GetProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject switch
        {
            null => Result.Error<JsonNode>("JSON object is null."),
            _ => jsonObject.TryGetPropertyValue(propertyName, out var jsonNode)
                    ? jsonNode switch
                    {
                        null => Result.Error<JsonNode>($"Property '{propertyName}' is null."),
                        _ => jsonNode
                    }
                    : Error.From($"JSON object does not have a property named '{propertyName}'.")
        };

    public static Option<JsonNode> GetOptionalProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.GetProperty(propertyName)
                  .Match(Option<JsonNode>.Some,
                         _ => Option.None);

    public static Result<T> GetProperty<T>(this JsonObject? jsonObject, string propertyName, Func<JsonNode, Result<T>> selector) =>
        jsonObject.GetProperty(propertyName)
                  .Bind(selector)
                  .AddPropertyNameToErrorMessage(propertyName);

    public static Result<JsonObject> GetJsonObjectProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.GetProperty(propertyName,
                               jsonNode => jsonNode.AsJsonObject());

    private static Result<T> AddPropertyNameToErrorMessage<T>(this Result<T> result, string propertyName)
    {
        return result.MapError(replaceError);

        Error replaceError(Error error) =>
            Error.From($"Property '{propertyName}' is invalid. {error}");
    }

    public static Result<JsonArray> GetJsonArrayProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.GetProperty(propertyName,
                               jsonNode => jsonNode.AsJsonArray());

    public static Result<JsonValue> GetJsonValueProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.GetProperty(propertyName,
                               jsonNode => jsonNode.AsJsonValue());

    public static Result<string> GetStringProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.GetProperty(propertyName,
                               jsonNode => jsonNode.AsJsonValue()
                                                   .Bind(jsonValue => jsonValue.AsString()));

    public static Result<int> GetIntProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.GetProperty(propertyName,
                               jsonNode => jsonNode.AsJsonValue()
                                                   .Bind(jsonValue => jsonValue.AsInt()));

    public static Result<bool> GetBoolProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.GetProperty(propertyName,
                               jsonNode => jsonNode.AsJsonValue()
                                                   .Bind(jsonValue => jsonValue.AsBool()));

    public static Result<Guid> GetGuidProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.GetProperty(propertyName,
                               jsonNode => jsonNode.AsJsonValue()
                                                   .Bind(jsonValue => jsonValue.AsGuid()));

    public static Result<Uri> GetAbsoluteUriProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.GetProperty(propertyName,
                               jsonNode => jsonNode.AsJsonValue()
                                                   .Bind(jsonValue => jsonValue.AsAbsoluteUri()));

    /// <summary>
    /// Sets a property in the JSON object, leaving the original object unchanged and returning a new object.
    /// To mutate the original object (e.g. for performance reasons), set <paramref name="mutateOriginal"/> to true.
    /// </summary>
    public static JsonObject SetProperty(this JsonObject jsonObject, string propertyName, JsonNode? propertyValue, bool mutateOriginal = false)
    {
        var newJson = mutateOriginal
                        ? jsonObject
                        : jsonObject.DeepClone().AsObject();

        newJson[propertyName] = propertyValue;

        return newJson;
    }

    /// <summary>
    /// Removes a property from the JSON object, leaving the original object unchanged and returning a new object.
    /// To mutate the original object (e.g. for performance reasons), set <paramref name="mutateOriginal"/> to true.
    /// </summary>
    public static JsonObject RemoveProperty(this JsonObject jsonObject, string propertyName, bool mutateOriginal = false)
    {
        var newJson = mutateOriginal
                        ? jsonObject
                        : jsonObject.DeepClone().AsObject();

        newJson.Remove(propertyName);

        return newJson;
    }

    public static Result<JsonObject> ToJsonObject(BinaryData? data, JsonSerializerOptions? options = default) =>
        JsonNodeModule.Deserialize<JsonObject>(data, options);
}