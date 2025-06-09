using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace common;

/// <summary>
/// Provides extension methods for working with <see cref="JsonValue"/> instances in a functional style.
/// </summary>
public static class JsonValueModule
{
    /// <summary>
    /// Safely converts a <see cref="JsonValue"/> to a string.
    /// </summary>
    /// <param name="jsonValue">The <see cref="JsonValue"/> to convert.</param>
    /// <returns>Success with the <see cref="string"/> value if <see cref="JsonValue"/> contains a string, otherwise error details.</returns>
    public static Result<string> AsString(this JsonValue? jsonValue) =>
        jsonValue?.GetValueKind() switch
        {
            JsonValueKind.String => jsonValue.GetStringValue() switch
            {
                null => Result.Success("JSON value has a null string."),
                var stringValue => stringValue
            },
            _ => Error.From("JSON value is not a string.")
        };
        
    private static string? GetStringValue(this JsonValue? jsonValue) =>
        jsonValue?.GetValue<object>().ToString();

    /// <summary>
    /// Safely converts a <see cref="JsonValue"/> to an integer.
    /// </summary>
    /// <param name="jsonValue">The <see cref="JsonValue"/> to convert.</param>
    /// <returns>Success with the <see cref="int"/> value if <see cref="JsonValue"/> contains a valid integer, otherwise error details.</returns>
    public static Result<int> AsInt(this JsonValue? jsonValue)
    {
        var errorMessage = "JSON value is not an integer.";

        return jsonValue?.GetValueKind() switch
        {
            JsonValueKind.Number => int.TryParse(jsonValue.GetStringValue(), out var result)
                                    ? Result.Success(result)
                                    : Error.From(errorMessage),
            _ => Error.From(errorMessage)
        };
    }

    /// <summary>
    /// Safely converts a <see cref="JsonValue"/> to a boolean.
    /// </summary>
    /// <param name="jsonValue">The <see cref="JsonValue"/> to convert.</param>
    /// <returns>Success with the <see cref="bool"/> value if <see cref="JsonValue"/> contains a boolean, otherwise error details.</returns>
    public static Result<bool> AsBool(this JsonValue? jsonValue) =>
        jsonValue?.GetValueKind() switch
        {
            JsonValueKind.True => Result.Success(true),
            JsonValueKind.False => false,
            _ => Error.From("JSON value is not a boolean.")
        };

    /// <summary>
    /// Safely converts a <see cref="JsonValue"/> to a GUID.
    /// </summary>
    /// <param name="jsonValue">The <see cref="JsonValue"/> to convert.</param>
    /// <returns>Success with the <see cref="Guid"/> value if <see cref="JsonValue"/> contains a valid GUID string, otherwise error details.</returns>
    public static Result<Guid> AsGuid(this JsonValue? jsonValue)
    {
        var errorMessage = "JSON value is not a GUID.";

        return jsonValue.AsString()
                        .Bind(stringValue => Guid.TryParse(jsonValue.GetStringValue(), out var result)
                                                ? Result.Success(result)
                                                : Error.From(errorMessage))
                        .MapError(_ => errorMessage);
    }

    /// <summary>
    /// Safely converts a <see cref="JsonValue"/> to an absolute URI.
    /// </summary>
    /// <param name="jsonValue">The <see cref="JsonValue"/> to convert.</param>
    /// <returns>Success with the <see cref="Uri"/> value if <see cref="JsonValue"/> contains a valid absolute URI string, otherwise error details.</returns>
    public static Result<Uri> AsAbsoluteUri(this JsonValue? jsonValue)
    {
        var errorMessage = "JSON value is not an absolute URI.";

        return jsonValue.AsString()
                        .Bind(stringValue => Uri.TryCreate(jsonValue.GetStringValue(), UriKind.Absolute, out var result)
                                                ? Result.Success(result)
                                                : Error.From(errorMessage))
                        .MapError(_ => errorMessage);
    }
}