using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace common;

public static class JsonValueModule
{
    public static JsonResult<string> AsString(this JsonValue? jsonValue) =>
        jsonValue?.GetValueKind() switch
        {
            JsonValueKind.String => jsonValue.GetStringValue() switch
            {
                null => JsonResult.Fail<string>("JSON value has a null string."),
                var stringValue => JsonResult.Succeed(stringValue)
            },
            _ => JsonResult.Fail<string>("JSON value is not a string.")
        };

    private static string? GetStringValue(this JsonValue? jsonValue) =>
        jsonValue?.GetValue<object>().ToString();

    public static JsonResult<int> AsInt(this JsonValue? jsonValue)
    {
        var errorMessage = "JSON value is not an integer.";

        return jsonValue?.GetValueKind() switch
        {
            JsonValueKind.Number => int.TryParse(jsonValue.GetStringValue(), out var result)
                                    ? JsonResult.Succeed(result)
                                    : JsonResult.Fail<int>(errorMessage),
            _ => JsonResult.Fail<int>(errorMessage)
        };
    }

    public static JsonResult<bool> AsBool(this JsonValue? jsonValue) =>
        jsonValue?.GetValueKind() switch
        {
            JsonValueKind.True => JsonResult.Succeed(true),
            JsonValueKind.False => JsonResult.Succeed(false),
            _ => JsonResult.Fail<bool>("JSON value is not a boolean.")
        };

    public static JsonResult<Guid> AsGuid(this JsonValue? jsonValue)
    {
        var errorMessage = "JSON value is not a GUID.";

        return jsonValue.AsString()
                        .Bind(stringValue => Guid.TryParse(jsonValue.GetStringValue(), out var result)
                                            ? JsonResult.Succeed(result)
                                            : JsonResult.Fail<Guid>(errorMessage))
                        .ReplaceError(errorMessage);
    }

    public static JsonResult<Uri> AsAbsoluteUri(this JsonValue? jsonValue)
    {
        var errorMessage = "JSON value is not an absolute URI.";

        return jsonValue.AsString()
                 .Bind(stringValue => Uri.TryCreate(jsonValue.GetStringValue(), UriKind.Absolute, out var result)
                                        ? JsonResult.Succeed(result)
                                        : JsonResult.Fail<Uri>(errorMessage))
                 .ReplaceError(errorMessage);
    }
}