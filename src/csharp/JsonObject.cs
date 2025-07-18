using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace common;

/// <summary>
/// Provides extension methods for working with <see cref="JsonObject"/> instances in a functional style.
/// </summary>
public static class JsonObjectModule
{
    /// <summary>
    /// Safely retrieves a property value from a <see cref="JsonObject"/>.
    /// </summary>
    /// <param name="jsonObject">The <see cref="JsonObject"/> to query.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>Success with the <see cref="JsonNode"/> property value if found, otherwise error details.</returns>
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

    /// <summary>
    /// Safely retrieves an optional property value from a <see cref="JsonObject"/>.
    /// </summary>
    /// <param name="jsonObject">The <see cref="JsonObject"/> to query.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>Some with the <see cref="JsonNode"/> if found, otherwise None.</returns>
    public static Option<JsonNode> GetOptionalProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.GetProperty(propertyName)
                  .Match(Option<JsonNode>.Some,
                         _ => Option.None);

    /// <summary>
    /// Safely retrieves and transforms a property value from a <see cref="JsonObject"/> using a selector function.
    /// </summary>
    /// <typeparam name="T">The type to transform into.</typeparam>
    /// <param name="jsonObject">The <see cref="JsonObject"/> to query.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="selector">Function that transforms the <see cref="JsonNode"/> into the desired type.</param>
    /// <returns>Success with the transformed value if property exists and transformation succeeds, otherwise error with property context.</returns>
    public static Result<T> GetProperty<T>(this JsonObject? jsonObject, string propertyName, Func<JsonNode, Result<T>> selector) =>
        jsonObject.GetProperty(propertyName)
                  .Bind(selector)
                  .AddPropertyNameToErrorMessage(propertyName);

    /// <summary>
    /// Safely retrieves a <see cref="JsonObject"/> property from a <see cref="JsonObject"/>.
    /// </summary>
    /// <param name="jsonObject">The <see cref="JsonObject"/> to query.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>Success with the <see cref="JsonObject"/> if found and is an object, otherwise error details.</returns>
    public static Result<JsonObject> GetJsonObjectProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.GetProperty(propertyName,
                               jsonNode => jsonNode.AsJsonObject());

    /// <summary>
    /// Adds property name context to error messages for better diagnostics.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="result">The result to enhance.</param>
    /// <param name="propertyName">The property name to include in errors.</param>
    /// <returns>The original result if successful, otherwise error with enhanced context.</returns>
    private static Result<T> AddPropertyNameToErrorMessage<T>(this Result<T> result, string propertyName)
    {
        return result.MapError(replaceError);

        Error replaceError(Error error) =>
            Error.From($"Property '{propertyName}' is invalid. {error}");
    }

    /// <summary>
    /// Safely retrieves a <see cref="JsonArray"/> property from a <see cref="JsonObject"/>.
    /// </summary>
    /// <param name="jsonObject">The <see cref="JsonObject"/> to query.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>Success with the <see cref="JsonArray"/> if found and is an array, otherwise error details.</returns>
    public static Result<JsonArray> GetJsonArrayProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.GetProperty(propertyName,
                               jsonNode => jsonNode.AsJsonArray());

    /// <summary>
    /// Safely retrieves a <see cref="JsonValue"/> property from a <see cref="JsonObject"/>.
    /// </summary>
    /// <param name="jsonObject">The <see cref="JsonObject"/> to query.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>Success with the <see cref="JsonValue"/> if found and is a value, otherwise error details.</returns>
    public static Result<JsonValue> GetJsonValueProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.GetProperty(propertyName,
                               jsonNode => jsonNode.AsJsonValue());

    /// <summary>
    /// Safely retrieves a string property from a <see cref="JsonObject"/>.
    /// </summary>
    /// <param name="jsonObject">The <see cref="JsonObject"/> to query.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>Success with the <see cref="string"/> value if found and is a string, otherwise error details.</returns>
    public static Result<string> GetStringProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.GetProperty(propertyName,
                               jsonNode => jsonNode.AsJsonValue()
                                                   .Bind(jsonValue => jsonValue.AsString()));

    /// <summary>
    /// Safely retrieves an integer property from a <see cref="JsonObject"/>.
    /// </summary>
    /// <param name="jsonObject">The <see cref="JsonObject"/> to query.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>Success with the <see cref="int"/> value if found and is a valid integer, otherwise error details.</returns>
    public static Result<int> GetIntProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.GetProperty(propertyName,
                               jsonNode => jsonNode.AsJsonValue()
                                                   .Bind(jsonValue => jsonValue.AsInt()));

    /// <summary>
    /// Safely retrieves a boolean property from a <see cref="JsonObject"/>.
    /// </summary>
    /// <param name="jsonObject">The <see cref="JsonObject"/> to query.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>Success with the <see cref="bool"/> value if found and is a boolean, otherwise error details.</returns>
    public static Result<bool> GetBoolProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.GetProperty(propertyName,
                               jsonNode => jsonNode.AsJsonValue()
                                                   .Bind(jsonValue => jsonValue.AsBool()));

    /// <summary>
    /// Safely retrieves a GUID property from a <see cref="JsonObject"/>.
    /// </summary>
    /// <param name="jsonObject">The <see cref="JsonObject"/> to query.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>Success with the <see cref="Guid"/> value if found and is a valid GUID string, otherwise error details.</returns>
    public static Result<Guid> GetGuidProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.GetProperty(propertyName,
                               jsonNode => jsonNode.AsJsonValue()
                                                   .Bind(jsonValue => jsonValue.AsGuid()));

    /// <summary>
    /// Safely retrieves an absolute URI property from a <see cref="JsonObject"/>.
    /// </summary>
    /// <param name="jsonObject">The <see cref="JsonObject"/> to query.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>Success with the <see cref="Uri"/> value if found and is a valid absolute URI string, otherwise error details.</returns>
    public static Result<Uri> GetAbsoluteUriProperty(this JsonObject? jsonObject, string propertyName) =>
        jsonObject.GetProperty(propertyName,
                               jsonNode => jsonNode.AsJsonValue()
                                                   .Bind(jsonValue => jsonValue.AsAbsoluteUri()));

    /// <summary>
    /// Sets a property in the JSON object.
    /// </summary>
    /// <param name="jsonObject">The <see cref="JsonObject"/> to modify.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="propertyValue">The property value.</param>
    /// <param name="mutateOriginal">If true, modifies the original; if false, creates a deep copy first.</param>
    /// <returns>A <see cref="JsonObject"/> with the property set.</returns>
    public static JsonObject SetProperty(this JsonObject jsonObject, string propertyName, JsonNode? propertyValue, bool mutateOriginal = false)
    {
        var newJson = mutateOriginal
                        ? jsonObject
                        : jsonObject.DeepClone().AsObject();

        newJson[propertyName] = propertyValue;

        return newJson;
    }

    /// <summary>
    /// Removes a property from the JSON object.
    /// </summary>
    /// <param name="jsonObject">The <see cref="JsonObject"/> to modify.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="mutateOriginal">If true, modifies the original; if false, creates a deep copy first.</param>
    /// <returns>A <see cref="JsonObject"/> with the property removed.</returns>
    public static JsonObject RemoveProperty(this JsonObject jsonObject, string propertyName, bool mutateOriginal = false)
    {
        var newJson = mutateOriginal
                        ? jsonObject
                        : jsonObject.DeepClone().AsObject();

        newJson.Remove(propertyName);

        return newJson;
    }

    /// <summary>
    /// Merges another <see cref="JsonObject"/> into the current one.
    /// </summary>
    /// <param name="original">The original <see cref="JsonObject"/> to merge into.</param>
    /// <param name="other">The <see cref="JsonObject"/> to merge from. If null, returns the original object.</param>
    /// <param name="mutateOriginal">If true, modifies the original; if false, creates a deep copy first.</param>
    /// <returns>A <see cref="JsonObject"/> with properties from both objects, with <paramref name="other"/> taking precedence for duplicate keys.</returns>
    public static JsonObject MergeWith(this JsonObject original, JsonObject? other, bool mutateOriginal = false)
    {
        if (other is null || other.Count == 0)
        {
            return original;
        }

        var mergedJson = mutateOriginal
                            ? original
                            : original.DeepClone().AsObject();

        foreach (var kvp in other)
        {
            mergedJson[kvp.Key] = kvp.Value?.DeepClone();
        }

        return mergedJson;
    }

    /// <summary>
    /// Deserializes binary data into a <see cref="JsonObject"/>.
    /// </summary>
    /// <param name="data">The <see cref="BinaryData"/> containing JSON.</param>
    /// <param name="options">Serializer options. Defaults to <see cref="JsonSerializerOptions.Web"/> if null.</param>
    /// <returns>Success with the deserialized <see cref="JsonObject"/>, or error if deserialization fails.</returns>
    public static Result<JsonObject> From(BinaryData? data, JsonSerializerOptions? options = default) =>
    JsonNodeModule.Deserialize<JsonObject>(data, options);
}