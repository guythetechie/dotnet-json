using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace common;

public static class JsonValueModule
{
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

    public static Result<bool> AsBool(this JsonValue? jsonValue) =>
        jsonValue?.GetValueKind() switch
        {
            JsonValueKind.True => Result.Success(true),
            JsonValueKind.False => false,
            _ => Error.From("JSON value is not a boolean.")
        };

    public static Result<Guid> AsGuid(this JsonValue? jsonValue)
    {
        var errorMessage = "JSON value is not a GUID.";

        return jsonValue.AsString()
                        .Bind(stringValue => Guid.TryParse(jsonValue.GetStringValue(), out var result)
                                                ? Result.Success(result)
                                                : Error.From(errorMessage))
                        .MapError(_ => errorMessage);
    }

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