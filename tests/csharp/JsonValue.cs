using CsCheck;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace common.tests;

public class JsonValueModule_AsString_Tests()
{
    [Test]
    public async ValueTask Fails_if_the_node_is_null()
    {
        // Act
        var result = ((JsonValue?)null).AsString();

        // Assert
        await Assert.That(result).IsError();
    }

    [Test]
    public async ValueTask Fails_if_the_value_is_not_a_string()
    {
        var gen = from jsonValue in JsonValueGenerator.Any
                  where jsonValue.GetValueKind() != JsonValueKind.String
                  select jsonValue;

        // Act & Assert
        await gen.SampleAsync(async jsonValue =>
        {
            // Act
            var result = jsonValue.AsString();

            // Assert
            await Assert.That(result).IsError();
        });
    }

    [Test]
    public async ValueTask Succeeds_if_the_value_is_a_string()
    {
        var gen = from str in Gen.String
                  let jsonValue = JsonValue.Create(str)
                  select (str, jsonValue);

        // Act & Assert
        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (str, jsonValue) = tuple;

            // Act
            var result = jsonValue.AsString();

            // Assert
            await Assert.That(result)
                        .IsSuccess()
                        .WhoseValue
                        .IsEquivalentTo(str);
        });
    }
}

public class JsonValueModule_AsInt_Tests
{
    [Test]
    public async ValueTask Fails_if_the_node_is_null()
    {
        // Act
        var result = ((JsonValue?)null).AsInt();

        // Assert
        await Assert.That(result).IsError();
    }

    [Test]
    public async ValueTask Fails_if_the_value_is_not_an_int()
    {
        var gen = from jsonValue in JsonValueGenerator.Any
                  where jsonValue.GetValueKind() != JsonValueKind.Number
                        || int.TryParse(jsonValue.ToObjectString(), out var _) is false
                  select jsonValue;

        await gen.SampleAsync(async jsonValue =>
        {
            // Act
            var result = jsonValue.AsInt();

            // Assert
            await Assert.That(result).IsError();
        });
    }

    [Test]
    public async ValueTask Succeeds_if_the_value_is_an_int()
    {
        var gen = from integer in Gen.Int
                  let value = JsonValue.Create(integer)
                  select (integer, value);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (integer, value) = tuple;

            // Act
            var result = value.AsInt();

            // Assert
            await Assert.That(result)
                        .IsSuccess()
                        .WhoseValue
                        .IsEquivalentTo(integer);
        });
    }
}

public class JsonValueModule_AsBool_Tests
{
    [Test]
    public async ValueTask Fails_if_the_node_is_null()
    {
        // Act
        var result = ((JsonValue?)null).AsBool();

        // Assert
        await Assert.That(result).IsError();
    }

    [Test]
    public async ValueTask Fails_if_the_value_is_not_a_boolean()
    {
        var gen = from jsonValue in JsonValueGenerator.Any
                  where jsonValue.GetValueKind() is not (JsonValueKind.True or JsonValueKind.False)
                  select jsonValue;

        await gen.SampleAsync(async jsonValue =>
        {
            // Act
            var result = jsonValue.AsBool();

            // Assert
            await Assert.That(result).IsError();
        });
    }

    [Test]
    public async ValueTask Succeeds_if_the_value_is_a_boolean()
    {
        var gen = from boolean in Gen.Bool
                  let value = JsonValue.Create(boolean)
                  select (boolean, value);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (boolean, value) = tuple;

            // Act
            var result = value.AsBool();

            // Assert
            await Assert.That(result)
                        .IsSuccess()
                        .WhoseValue
                        .IsEquivalentTo(boolean);
        });
    }
}

public class JsonValueModule_AsGuid_Tests
{
    [Test]
    public async ValueTask Fails_if_the_node_is_null()
    {
        // Act
        var result = ((JsonValue?)null).AsGuid();

        // Assert
        await Assert.That(result).IsError();
    }

    [Test]
    public async ValueTask Fails_if_the_value_is_not_a_valid_guid_string()
    {
        var gen =
            from jsonValue in JsonValueGenerator.Any
            where jsonValue.GetValueKind() != JsonValueKind.String
                  || Guid.TryParse(jsonValue.ToObjectString(), out var _) is false
            select jsonValue;

        await gen.SampleAsync(async jsonValue =>
        {
            // Act
            var result = jsonValue.AsGuid();

            // Assert
            await Assert.That(result).IsError();
        });
    }

    [Test]
    public async ValueTask Succeeds_if_the_value_is_a_valid_guid_string()
    {
        var gen =
            from guid in Gen.Guid
            let jsonValue = JsonValue.Create(guid.ToString())
            select (guid, jsonValue);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (guid, jsonValue) = tuple;

            // Act
            var result = jsonValue.AsGuid();

            // Assert
            await Assert.That(result)
                        .IsSuccess()
                        .WhoseValue
                        .IsEquivalentTo(guid);
        });
    }
}

public class JsonValueModule_AsUInt_Tests
{
    [Test]
    public async ValueTask Fails_if_the_node_is_null()
    {
        // Act
        var result = ((JsonValue?)null).AsUInt();

        // Assert
        await Assert.That(result).IsError();
    }

    [Test]
    public async ValueTask Fails_if_the_value_is_not_a_valid_uint()
    {
        var gen =
            from jsonValue in JsonValueGenerator.Any
            where jsonValue.GetValueKind() != JsonValueKind.Number
                  || uint.TryParse(jsonValue.ToObjectString(), out var _) is false
            select jsonValue;

        await gen.SampleAsync(async jsonValue =>
        {
            // Act
            var result = jsonValue.AsUInt();

            // Assert
            await Assert.That(result).IsError();
        });
    }

    [Test]
    public async ValueTask Succeeds_if_the_value_is_a_valid_uint()
    {
        var gen =
            from uinteger in Gen.UInt
            let jsonValue = JsonValue.Create(uinteger)
            select (uinteger, jsonValue);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (uinteger, jsonValue) = tuple;

            // Act
            var result = jsonValue.AsUInt();

            // Assert
            await Assert.That(result)
                        .IsSuccess()
                        .WhoseValue
                        .IsEquivalentTo(uinteger);
        });
    }
}

public class JsonValueModule_AsDateTimeOffset_Tests
{
    [Test]
    public async ValueTask Fails_if_the_node_is_null()
    {
        // Act
        var result = ((JsonValue?)null).AsDateTimeOffset();

        // Assert
        await Assert.That(result).IsError();
    }

    [Test]
    public async ValueTask Fails_if_the_value_is_not_a_valid_DateTimeOffset_string()
    {
        var gen =
            from jsonValue in JsonValueGenerator.Any
            where jsonValue.GetValueKind() != JsonValueKind.String
                  || DateTimeOffset.TryParse(jsonValue.ToObjectString(), out var _) is false
            select jsonValue;

        await gen.SampleAsync(async jsonValue =>
        {
            // Act
            var result = jsonValue.AsDateTimeOffset();

            // Assert
            await Assert.That(result).IsError();
        });
    }

    [Test]
    public async ValueTask Succeeds_if_the_value_is_a_valid_DateTimeOffset_string()
    {
        var gen =
            from dateTimeOffset in Gen.DateTimeOffset
            let jsonValue = JsonValue.Create(dateTimeOffset.ToString("o"))
            select (dateTimeOffset, jsonValue);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (dateTimeOffset, jsonValue) = tuple;

            // Act
            var result = jsonValue.AsDateTimeOffset();

            // Assert
            await Assert.That(result)
                        .IsSuccess()
                        .WhoseValue
                        .IsEquivalentTo(dateTimeOffset);
        });
    }
}

public class JsonValueModule_AsAbsoluteUri_Tests
{
    [Test]
    public async ValueTask Fails_if_the_node_is_null()
    {
        // Act
        var result = ((JsonValue?)null).AsAbsoluteUri();

        // Assert
        await Assert.That(result).IsError();
    }

    [Test]
    public async ValueTask Fails_if_the_value_is_not_a_valid_absolute_uri_string()
    {
        var gen =
            from jsonValue in JsonValueGenerator.Any
            where jsonValue.GetValueKind() != JsonValueKind.String
                  || Uri.TryCreate(jsonValue.ToObjectString(), UriKind.Absolute, out var _) is false
            select jsonValue;

        await gen.SampleAsync(async jsonValue =>
        {
            // Act
            var result = jsonValue.AsAbsoluteUri();

            // Assert
            await Assert.That(result).IsError();
        });
    }

    [Test]
    public async ValueTask Succeeds_if_the_value_is_a_valid_absolute_uri_string()
    {
        var gen =
            from uri in HttpGenerator.AbsoluteUri
            let jsonValue = JsonValue.Create(uri.ToString())
            select (uri, jsonValue);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (uri, jsonValue) = tuple;

            // Act
            var result = jsonValue.AsAbsoluteUri();

            // Assert
            await Assert.That(result)
                        .IsSuccess()
                        .WhoseValue
                        .IsEquivalentTo(uri);
        });
    }
}

file static class JsonValueExtensions
{
    public static string? ToObjectString(this JsonValue jsonValue) =>
        jsonValue.GetValue<object>().ToString();
}