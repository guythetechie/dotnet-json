using CsCheck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace common.tests;

file static class Common
{
    public static CancellationToken CancellationToken =>
        TestContext.Current?.Execution.CancellationToken ?? CancellationToken.None;

    public static Gen<string> GeneratePropertyName() =>
        from str in Gen.String.AlphaNumeric
        where string.IsNullOrWhiteSpace(str) is false
        select str;
}

public class JsonObjectModule_GetProperty_Tests
{
    [Test]
    public async ValueTask Fails_if_the_property_is_missing()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any.Null()
                where jsonObject is null || jsonObject.ContainsKey(propertyName) is false
                select jsonObject
            select (propertyName, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }

    [Test]
    public async ValueTask Fails_if_the_property_value_is_null()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any
                select jsonObject.SetProperty(propertyName, null)
            select (propertyName, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }

    [Test]
    public async ValueTask Succeeds_if_the_property_is_present()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from propertyValue in JsonNodeGenerator.Any
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any
                select jsonObject.SetProperty(propertyName, propertyValue)
            select (propertyName, propertyValue, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, propertyValue, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsSuccess()
                        .WhoseValue
                        .IsEquivalentTo(propertyValue);
        });
    }
}

public class JsonObjectModule_GetOptionalProperty_Tests
{
    [Test]
    public async ValueTask Returns_none_if_the_property_is_missing()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any.Null()
                where jsonObject is null || jsonObject.ContainsKey(propertyName) is false
                select jsonObject
            select (propertyName, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetOptionalProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsNone();
        });
    }

    [Test]
    public async ValueTask Returns_none_if_the_property_value_is_null()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any
                select jsonObject.SetProperty(propertyName, null)
            select (propertyName, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetOptionalProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsNone();
        });
    }

    [Test]
    public async ValueTask Returns_some_if_the_property_is_present()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from propertyValue in JsonNodeGenerator.Any
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any
                select jsonObject.SetProperty(propertyName, propertyValue)
            select (propertyName, propertyValue, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, propertyValue, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetOptionalProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsSome()
                        .WhoseValue
                        .IsEquivalentTo(propertyValue);
        });
    }
}

public class JsonObjectModule_GetJsonObjectProperty_Tests
{
    [Test]
    public async ValueTask Fails_if_the_property_is_missing()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any.Null()
                where jsonObject is null || jsonObject.ContainsKey(propertyName) is false
                select jsonObject
            select (propertyName, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetJsonObjectProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }

    [Test]
    public async ValueTask Fails_if_the_property_is_not_a_json_object()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from propertyValue in
                from node in JsonNodeGenerator.Any.Null()
                where node is not JsonObject
                select node
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any
                select jsonObject.SetProperty(propertyName, propertyValue)
            select (propertyName, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetJsonObjectProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }

    [Test]
    public async ValueTask Succeeds_if_the_property_is_a_json_object()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from propertyValue in JsonNodeGenerator.JsonObject
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any
                select jsonObject.SetProperty(propertyName, propertyValue)
            select (propertyName, propertyValue, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, propertyValue, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetJsonObjectProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsSuccess()
                        .WhoseValue
                        .IsEquivalentTo(propertyValue);
        });
    }
}

public class JsonObjectModule_GetJsonArrayProperty_Tests
{
    [Test]
    public async ValueTask Fails_if_the_property_is_missing()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any.Null()
                where jsonObject is null || jsonObject.ContainsKey(propertyName) is false
                select jsonObject
            select (propertyName, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetJsonArrayProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }

    [Test]
    public async ValueTask Fails_if_the_property_is_not_a_json_array()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from propertyValue in
                from node in JsonNodeGenerator.Any.Null()
                where node is not JsonArray
                select node
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any
                select jsonObject.SetProperty(propertyName, propertyValue)
            select (propertyName, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetJsonArrayProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }

    [Test]
    public async ValueTask Succeeds_if_the_property_is_a_json_array()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from propertyValue in JsonNodeGenerator.JsonArray
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any
                select jsonObject.SetProperty(propertyName, propertyValue)
            select (propertyName, propertyValue, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, propertyValue, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetJsonArrayProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsSuccess()
                        .WhoseValue
                        .IsEquivalentTo(propertyValue);
        });
    }
}

public class JsonObjectModule_GetJsonValueProperty_Tests
{
    [Test]
    public async ValueTask Fails_if_the_property_is_missing()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any.Null()
                where jsonObject is null || jsonObject.ContainsKey(propertyName) is false
                select jsonObject
            select (propertyName, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetJsonValueProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }

    [Test]
    public async ValueTask Fails_if_the_property_is_not_a_json_value()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from propertyValue in
                from node in JsonNodeGenerator.Any.Null()
                where node is not JsonValue
                select node
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any
                select jsonObject.SetProperty(propertyName, propertyValue)
            select (propertyName, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetJsonValueProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }

    [Test]
    public async ValueTask Succeeds_if_the_property_is_a_json_value()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from propertyValue in JsonNodeGenerator.JsonValue
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any
                select jsonObject.SetProperty(propertyName, propertyValue)
            select (propertyName, propertyValue, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, propertyValue, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetJsonValueProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsSuccess()
                        .WhoseValue
                        .IsEquivalentTo(propertyValue);
        });
    }
}

public class JsonObjectModule_GetStringProperty_Tests
{
    [Test]
    public async ValueTask Fails_if_the_property_is_missing()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any.Null()
                where jsonObject is null || jsonObject.ContainsKey(propertyName) is false
                select jsonObject
            select (propertyName, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetStringProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }

    [Test]
    public async ValueTask Fails_if_the_property_is_not_a_string()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from propertyValue in
                from node in JsonNodeGenerator.Any.Null()
                where node is not JsonValue jsonValue
                      || jsonValue.GetValueKind() is not JsonValueKind.String
                select node
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any
                select jsonObject.SetProperty(propertyName, propertyValue)
            select (propertyName, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetStringProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }

    [Test]
    public async ValueTask Succeeds_if_the_property_is_a_string()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from propertyValue in Gen.String
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any
                select jsonObject.SetProperty(propertyName, propertyValue)
            select (propertyName, propertyValue, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, propertyValue, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetStringProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsSuccess()
                        .WhoseValue
                        .IsEquivalentTo(propertyValue);
        });
    }
}

public class JsonObjectModule_GetIntProperty_Tests
{
    [Test]
    public async ValueTask Fails_if_the_property_is_missing()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any.Null()
                where jsonObject is null || jsonObject.ContainsKey(propertyName) is false
                select jsonObject
            select (propertyName, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetIntProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }

    [Test]
    public async ValueTask Fails_if_the_property_is_not_an_int()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from propertyValue in
                from node in JsonNodeGenerator.Any.Null()
                where node is not JsonValue jsonValue
                      || jsonValue.GetValueKind() is not JsonValueKind.Number
                      || jsonValue.TryGetValue<int>(out _) is false
                select node
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any
                select jsonObject.SetProperty(propertyName, propertyValue)
            select (propertyName, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetIntProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }

    [Test]
    public async ValueTask Succeeds_if_the_property_is_an_int()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from propertyValue in Gen.Int
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any
                select jsonObject.SetProperty(propertyName, propertyValue)
            select (propertyName, propertyValue, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, propertyValue, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetIntProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsSuccess()
                        .WhoseValue
                        .IsEquivalentTo(propertyValue);
        });
    }
}

public class JsonObjectModule_GetBoolProperty_Tests
{
    [Test]
    public async ValueTask Fails_if_the_property_is_missing()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any.Null()
                where jsonObject is null || jsonObject.ContainsKey(propertyName) is false
                select jsonObject
            select (propertyName, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetBoolProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }

    [Test]
    public async ValueTask Fails_if_the_property_is_not_a_bool()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from propertyValue in
                from node in JsonNodeGenerator.Any.Null()
                where node is not JsonValue jsonValue
                      || jsonValue.GetValueKind() is not (JsonValueKind.True or JsonValueKind.False)
                select node
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any
                select jsonObject.SetProperty(propertyName, propertyValue)
            select (propertyName, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetBoolProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }

    [Test]
    public async ValueTask Succeeds_if_the_property_is_a_bool()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from propertyValue in Gen.Bool
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any
                select jsonObject.SetProperty(propertyName, propertyValue)
            select (propertyName, propertyValue, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, propertyValue, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetBoolProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsSuccess()
                        .WhoseValue
                        .IsEquivalentTo(propertyValue);
        });
    }
}

public class JsonObjectModule_GetGuidProperty_Tests
{
    [Test]
    public async ValueTask Fails_if_the_property_is_missing()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any.Null()
                where jsonObject is null || jsonObject.ContainsKey(propertyName) is false
                select jsonObject
            select (propertyName, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetGuidProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }

    [Test]
    public async ValueTask Fails_if_the_property_is_not_a_guid()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from propertyValue in
                from node in JsonNodeGenerator.Any.Null()
                where node is not JsonValue jsonValue
                      || jsonValue.GetValueKind() is not JsonValueKind.String
                      || (jsonValue.TryGetValue<Guid>(out _) is false
                          && (jsonValue.TryGetValue<string>(out var str) is false
                              || Guid.TryParse(str, out _) is false))
                select node
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any
                select jsonObject.SetProperty(propertyName, propertyValue)
            select (propertyName, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetGuidProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }

    [Test]
    public async ValueTask Succeeds_if_the_property_is_a_guid()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from propertyValue in Gen.Guid
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any
                select jsonObject.SetProperty(propertyName, propertyValue)
            select (propertyName, propertyValue, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, propertyValue, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetGuidProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsSuccess()
                        .WhoseValue
                        .IsEquivalentTo(propertyValue);
        });
    }
}

public class JsonObjectModule_GetUIntProperty_Tests
{
    [Test]
    public async ValueTask Fails_if_the_property_is_missing()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any.Null()
                where jsonObject is null || jsonObject.ContainsKey(propertyName) is false
                select jsonObject
            select (propertyName, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetUIntProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }

    [Test]
    public async ValueTask Fails_if_the_property_is_not_a_uint()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from propertyValue in
                from node in JsonNodeGenerator.Any.Null()
                where node is not JsonValue jsonValue
                      || jsonValue.GetValueKind() is not JsonValueKind.Number
                      || (jsonValue.TryGetValue<uint>(out _) is false && jsonValue.TryGetValue<int>(out _) is false)
                select node
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any
                select jsonObject.SetProperty(propertyName, propertyValue)
            select (propertyName, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetUIntProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }

    [Test]
    public async ValueTask Succeeds_if_the_property_is_a_uint()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from propertyValue in Gen.UInt
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any
                select jsonObject.SetProperty(propertyName, propertyValue)
            select (propertyName, propertyValue, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, propertyValue, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetUIntProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsSuccess()
                        .WhoseValue
                        .IsEquivalentTo(propertyValue);
        });
    }
}

public class JsonObjectModule_GetDateTimeOffsetProperty_Tests
{
    [Test]
    public async ValueTask Fails_if_the_property_is_missing()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any.Null()
                where jsonObject is null || jsonObject.ContainsKey(propertyName) is false
                select jsonObject
            select (propertyName, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetDateTimeOffsetProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }

    [Test]
    public async ValueTask Fails_if_the_property_is_not_a_datetimeoffset()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from propertyValue in
                from node in JsonNodeGenerator.Any.Null()
                where node is not JsonValue jsonValue
                      || jsonValue.GetValueKind() is not JsonValueKind.String
                      || (jsonValue.TryGetValue<DateTimeOffset>(out _) is false
                          && (jsonValue.TryGetValue<string>(out var str) is false
                              || DateTimeOffset.TryParse(str, out _) is false))
                select node
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any
                select jsonObject.SetProperty(propertyName, propertyValue)
            select (propertyName, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetDateTimeOffsetProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }

    [Test]
    public async ValueTask Succeeds_if_the_property_is_a_datetimeoffset()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from propertyValue in Gen.DateTimeOffset
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any
                select jsonObject.SetProperty(propertyName, propertyValue)
            select (propertyName, propertyValue, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, propertyValue, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetDateTimeOffsetProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsSuccess()
                        .WhoseValue
                        .IsEqualTo(propertyValue)
                        .Within(TimeSpan.FromSeconds(1)); // Allow some leeway for serialization/deserialization differences
        });
    }
}

public class JsonObjectModule_GetAbsoluteUriProperty_Tests
{
    [Test]
    public async ValueTask Fails_if_the_property_is_missing()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any.Null()
                where jsonObject is null || jsonObject.ContainsKey(propertyName) is false
                select jsonObject
            select (propertyName, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetAbsoluteUriProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }

    [Test]
    public async ValueTask Fails_if_the_property_is_not_an_absolute_uri()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from propertyValue in
                from node in JsonNodeGenerator.Any.Null()
                where node is not JsonValue jsonValue
                      || jsonValue.GetValueKind() is not JsonValueKind.String
                      || (jsonValue.TryGetValue<Uri>(out var uri) && uri.IsAbsoluteUri is false)
                      || (jsonValue.TryGetValue<string>(out var str) && (Uri.TryCreate(str, UriKind.Absolute, out _) is false))
                select node
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any
                select jsonObject.SetProperty(propertyName, propertyValue)
            select (propertyName, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetAbsoluteUriProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }

    [Test]
    public async ValueTask Succeeds_if_the_property_is_an_absolute_uri()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from propertyValue in HttpGenerator.AbsoluteUri
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any
                select jsonObject.SetProperty(propertyName, propertyValue.ToString())
            select (propertyName, propertyValue, jsonObject);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, propertyValue, jsonObject) = tuple;

            // Act
            var result = jsonObject.GetAbsoluteUriProperty(propertyName);

            // Assert
            await Assert.That(result)
                        .IsSuccess()
                        .WhoseValue
                        .IsEquivalentTo(propertyValue);
        });
    }
}

public class JsonObjectModule_SetProperty_Tests
{
    [Test]
    public async ValueTask Sets_the_property_value()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from propertyValue in JsonNodeGenerator.Any
            from jsonObject in JsonObjectGenerator.Any
            from mutateOriginal in Gen.Bool
            select (propertyName, propertyValue, jsonObject, mutateOriginal);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, propertyValue, jsonObject, mutateOriginal) = tuple;

            // Act
            var result = jsonObject.SetProperty(propertyName, propertyValue, mutateOriginal);

            // Assert
            await Assert.That<JsonObject>(result)
                        .HasProperty(propertyName)
                        .WhoseValue
                        .IsEquivalentTo(propertyValue);
        });
    }
}

public class JsonObjectModule_RemoveProperty_Tests
{
    [Test]
    public async ValueTask Removes_the_property()
    {
        var gen =
            from propertyName in Common.GeneratePropertyName()
            from propertyValue in JsonNodeGenerator.Any
            from jsonObject in
                from jsonObject in JsonObjectGenerator.Any
                select jsonObject.SetProperty(propertyName, propertyValue)
            from mutateOriginal in Gen.Bool
            select (propertyName, jsonObject, mutateOriginal);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (propertyName, jsonObject, mutateOriginal) = tuple;

            // Act
            var result = jsonObject.RemoveProperty(propertyName, mutateOriginal);

            // Assert
            await Assert.That<JsonObject>(result)
                        .DoesNotHaveProperty(propertyName);
        });
    }
}

public class JsonObjectModule_MergeWith_Tests
{
    [Test]
    public async ValueTask Result_contains_properties_from_both_objects()
    {
        var gen =
            from originalPropertyNames in GeneratePropertyName().HashSetOf()
            from otherPropertyNames in
                from sharedProperties in Generator.SubSetOf(originalPropertyNames)
                from otherProperties in GeneratePropertyName().HashSetOf()
                select sharedProperties.Union(otherProperties)
            from original in
                from kvps in Generator.Traverse(originalPropertyNames,
                                                property => from value in JsonNodeGenerator.Any.Null()
                                                            select KeyValuePair.Create(property, value))
                select new JsonObject(kvps)
            from other in
                from kvps in Generator.Traverse(otherPropertyNames,
                                                property => from value in JsonNodeGenerator.Any.Null()
                                                            select KeyValuePair.Create(property, value))
                from jsonObject in
                    kvps.IsEmpty
                    ? Gen.OneOfConst(new JsonObject(), null)
                    : Gen.Const<JsonObject?>(new JsonObject(kvps))
                select jsonObject
            from mutateOriginal in Gen.Bool
            select (originalPropertyNames, otherPropertyNames, original, other, mutateOriginal);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (originalPropertyNames, otherPropertyNames, original, other, mutateOriginal) = tuple;

            // Act
            var result = original.MergeWith(other, mutateOriginal);

            // Assert
            await Assert.That(result.ToDictionary().Keys)
                        .IsEquivalentTo(originalPropertyNames.Union(otherPropertyNames));
        });
    }

    private static Gen<string> GeneratePropertyName() =>
        from str in Gen.String
        where string.IsNullOrEmpty(str) is false
        select str;

    [Test]
    public async ValueTask Properties_from_other_object_take_precedence_over_original()
    {
        var gen =
            from sharedPropertyName in GeneratePropertyName()
            from originalValue in JsonNodeGenerator.Any.Null()
            from otherValue in JsonNodeGenerator.Any.Null()
            from original in
                from jsonObject in JsonObjectGenerator.Any
                select jsonObject.SetProperty(sharedPropertyName, originalValue)
            from other in
                from jsonObject in JsonObjectGenerator.Any
                select jsonObject.SetProperty(sharedPropertyName, otherValue)
            from mutateOriginal in Gen.Bool
            select (sharedPropertyName, original, other, mutateOriginal);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (sharedPropertyName, original, other, mutateOriginal) = tuple;

            // Act
            var result = original.MergeWith(other, mutateOriginal);

            // Assert
            await Assert.That<JsonObject>(result)
                        .HasProperty(sharedPropertyName)
                        .WhoseValue
                        .IsEquivalentTo(other[sharedPropertyName]);
        });
    }

    [Test]
    public async ValueTask Properties_from_original_that_are_not_in_other_are_preserved()
    {
        var gen =
            from originalPropertyNames in GeneratePropertyName().HashSetOf()
            from otherPropertyNames in
                from sharedProperties in Generator.SubSetOf(originalPropertyNames)
                from otherProperties in GeneratePropertyName().HashSetOf()
                select sharedProperties.Union(otherProperties)
            where originalPropertyNames.Except(otherPropertyNames).IsEmpty is false
            from original in
                from kvps in Generator.Traverse(originalPropertyNames,
                                                property => from value in JsonNodeGenerator.Any.Null()
                                                            select KeyValuePair.Create(property, value))
                select new JsonObject(kvps)
            from other in
                from kvps in Generator.Traverse(otherPropertyNames,
                                                property => from value in JsonNodeGenerator.Any.Null()
                                                            select KeyValuePair.Create(property, value))
                from jsonObject in
                    kvps.IsEmpty
                    ? Gen.OneOfConst(new JsonObject(), null)
                    : Gen.Const<JsonObject?>(new JsonObject(kvps))
                select jsonObject
            from mutateOriginal in Gen.Bool
            select (originalPropertyNames, otherPropertyNames, original, other, mutateOriginal);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (originalPropertyNames, otherPropertyNames, original, other, mutateOriginal) = tuple;

            // Act
            var result = original.MergeWith(other, mutateOriginal);

            // Assert
            var originalOnlyPropertyNames = originalPropertyNames.Except(otherPropertyNames);
            foreach (var propertyName in originalOnlyPropertyNames)
            {
                await Assert.That<JsonObject>(result)
                            .HasProperty(propertyName)
                            .WhoseValue
                            .IsEquivalentTo(original[propertyName]);
            }
        });
    }
}

public class JsonObjectModule_From_Tests
{
    [Test]
    public async ValueTask Fails_if_data_does_not_represent_a_json_object()
    {
        var gen =
            JsonNodeGenerator.Any
                             .Where(node => node is not JsonObject)
                             .Select(node => BinaryData.FromObjectAsJson(node))
                             .Null();

        await gen.SampleAsync(async data =>
        {
            // Act
            var result = JsonObjectModule.From(data);

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }

    [Test]
    public async ValueTask Roundtrips()
    {
        var gen = JsonNodeGenerator.JsonObject;

        await gen.SampleAsync(async jsonObject =>
        {
            // Arrange
            var data = BinaryData.FromObjectAsJson(jsonObject);

            // Act
            var result = JsonObjectModule.From(data);

            // Assert
            await Assert.That(result)
                        .IsSuccess()
                        .WhoseValue
                        .IsEquivalentTo(jsonObject);
        });
    }
}

// public class JsonObjectTests
// {

//     [Fact]
//     public void MergeWith_returns_original_object_when_other_is_null_or_empty()
//     {
//         var generator = from first in Generator.JsonObject
//                         from second in Gen.OneOfConst(new JsonObject(), null)
//                         select (first, second);

//         generator.Sample(x =>
//         {
//             var (first, second) = x;

//             var result = first.MergeWith(second);

//             result.Should().BeSameAs(first);
//         });
//     }

//     [Fact]
//     public void MergeWith_merges_properties_from_other_object()
//     {
//         var generator = from first in Generator.JsonObject
//                         from second in Generator.JsonObject
//                         select (first, second);

//         generator.Sample(x =>
//         {
//             var (first, second) = x;

//             var result = first.MergeWith(second);

//             // All properties from first should be present
//             first.Iter(kvp =>
//             {
//                 var (firstKey, firstValue) = kvp;

//                 if (second.ContainsKey(firstKey))
//                 {
//                     result.Should().ContainProperty(firstKey);
//                 }
//                 else
//                 {
//                     result.Should().ContainProperty(firstKey).Which?.Should().BeEquivalentTo(firstValue);
//                 }
//             });

//             // All properties from second should be present and take precedence
//             second.Iter(kvp =>
//             {
//                 var (secondKey, secondValue) = kvp;

//                 result.Should().ContainProperty(secondKey).Which?.Should().BeEquivalentTo(secondValue);
//             });
//         });
//     }

//     [Fact]
//     public void MergeWith_prioritizes_other_for_duplicate_keys()
//     {
//         var generator = from firstObject in JsonObjectGenerator.Value
//                         from secondObject in JsonObjectGenerator.Value
//                         from duplicatePropertyName in GeneratePropertyName()
//                         from firstValue in JsonNodeGenerator.Value
//                         from secondValue in JsonNodeGenerator.Value
//                         select (firstObject.SetProperty(duplicatePropertyName, firstValue),
//                                 secondObject.SetProperty(duplicatePropertyName, secondValue),
//                                 duplicatePropertyName,
//                                 secondValue);

//         generator.Sample(x =>
//         {
//             var (first, second, propertyName, expectedValue) = x;

//             var result = first.MergeWith(second);

//             result.Should().ContainProperty(propertyName).Which?.Should().BeEquivalentTo(expectedValue);
//         });
//     }

//     [Fact]
//     public void MergeWith_does_not_mutate_original_when_mutateOriginal_is_false()
//     {
//         var generator = from first in Generator.JsonObject
//                         from second in Generator.JsonObject
//                         where second.Count > 0
//                         select (first, second);

//         generator.Sample(x =>
//         {
//             var (first, second) = x;

//             var result = first.MergeWith(second, mutateOriginal: false);

//             result.Should().NotBeSameAs(first);
//         });
//     }

//     [Fact]
//     public void MergeWith_mutates_original_when_mutateOriginal_is_true()
//     {
//         var generator = from first in Generator.JsonObject
//                         from second in Generator.JsonObject.Null()
//                         select (first, second);

//         generator.Sample(x =>
//         {
//             var (first, second) = x;

//             var result = first.MergeWith(second, mutateOriginal: true);

//             result.Should().BeSameAs(first);
//         });
//     }

//     [Fact]
//     public void MergeWith_creates_deep_copies_of_values()
//     {
//         var generator = from first in Generator.JsonObject
//                         from second in Generator.JsonObject
//                         select (first, second);

//         generator.Sample(x =>
//         {
//             var (first, second) = x;

//             var result = first.MergeWith(second);

//             result.Iter(kvp =>
//             {
//                 var (key, value) = kvp;

//                 second.GetProperty(key)
//                       .Iter(secondValue => value?.Should().NotBeSameAs(secondValue));
//             });
//         });
//     }
// }
