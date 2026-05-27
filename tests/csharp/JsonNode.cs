using CsCheck;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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

    public static bool StringIsNotValidJson([NotNullWhen(false)] string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return true;
        }

        try
        {
            JsonNode.Parse(input);
            return false;
        }
        catch (JsonException)
        {
            return true;
        }
    }
}

public class JsonNodeModule_AsJsonObject_Tests
{
    [Test]
    public async ValueTask Fails_if_the_node_is_not_a_json_object()
    {
        var gen =
            from node in JsonNodeGenerator.Any.Null()
            where node is not JsonObject
            select node;

        await gen.SampleAsync(async node =>
        {
            // Act
            var result = node.AsJsonObject();

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }

    [Test]
    public async ValueTask Succeeds_if_the_node_is_a_json_object()
    {
        var gen = JsonObjectGenerator.Any;

        await gen.SampleAsync(async jsonObject =>
        {
            // Arrange
            var node = (JsonNode)jsonObject;

            // Act
            var result = node.AsJsonObject();

            // Assert
            await Assert.That(result)
                        .IsSuccess()
                        .WhoseValue
                        .IsEquivalentTo(jsonObject);
        });
    }
}

public class JsonNodeModule_AsJsonArray_Tests
{
    [Test]
    public async ValueTask Fails_if_the_node_is_not_a_json_array()
    {
        var gen =
            from node in JsonNodeGenerator.Any.Null()
            where node is not JsonArray
            select node;

        await gen.SampleAsync(async node =>
        {
            // Act
            var result = node.AsJsonArray();

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }

    [Test]
    public async ValueTask Succeeds_if_the_node_is_a_json_array()
    {
        var gen = JsonArrayGenerator.Any;

        await gen.SampleAsync(async jsonArray =>
        {
            // Arrange
            var node = (JsonNode)jsonArray;

            // Act
            var result = node.AsJsonArray();

            // Assert
            await Assert.That(result)
                        .IsSuccess()
                        .WhoseValue
                        .IsEquivalentTo(jsonArray);
        });
    }
}

public class JsonNodeModule_AsJsonValue_Tests
{
    [Test]
    public async ValueTask Fails_if_the_node_is_not_a_json_value()
    {
        var gen =
            from node in JsonNodeGenerator.Any.Null()
            where node is not JsonValue
            select node;

        await gen.SampleAsync(async node =>
        {
            // Act
            var result = node.AsJsonValue();

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }

    [Test]
    public async ValueTask Succeeds_if_the_node_is_a_json_value()
    {
        var gen = JsonValueGenerator.Any;

        await gen.SampleAsync(async jsonValue =>
        {
            // Arrange
            var node = (JsonNode)jsonValue;

            // Act
            var result = node.AsJsonValue();

            // Assert
            await Assert.That(result)
                        .IsSuccess()
                        .WhoseValue
                        .IsEquivalentTo(jsonValue);
        });
    }
}

public class JsonNodeModule_From_Stream_Tests
{
    [Test]
    public async ValueTask Fails_if_the_stream_is_not_valid_json()
    {
        var gen =
            Gen.String
               .Where(Common.StringIsNotValidJson)
               .Select(BinaryData.FromString)
               .Null();

        await gen.SampleAsync(async data =>
        {
            // Arrange
            using var stream = data?.ToStream();

            // Act
            var result = await JsonNodeModule.From(stream, default, default, Common.CancellationToken);

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }

    [Test]
    public async ValueTask Roundtrips()
    {
        var gen = JsonNodeGenerator.Any;

        await gen.SampleAsync(async node =>
        {
            // Arrange
            using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, node, options: default, Common.CancellationToken);
            stream.Seek(0, SeekOrigin.Begin);

            // Act
            var result = await JsonNodeModule.From(stream, default, default, Common.CancellationToken);

            // Assert
            await Assert.That(result)
                        .IsSuccess()
                        .WhoseValue
                        .IsEquivalentTo(node);
        });
    }
}

public class JsonNodeModule_From_BinaryData_Tests
{
    [Test]
    public async ValueTask Fails_if_the_binary_data_is_not_valid_json()
    {
        var gen =
            Gen.String
               .Where(Common.StringIsNotValidJson)
               .Select(BinaryData.FromString)
               .Null();

        await gen.SampleAsync(async data =>
        {
            // Act
            var result = JsonNodeModule.From(data);

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }

    [Test]
    public async ValueTask Roundtrips()
    {
        var gen = JsonNodeGenerator.Any;

        await gen.SampleAsync(async node =>
        {
            // Arrange
            var bytes = JsonSerializer.SerializeToUtf8Bytes(node);
            var data = BinaryData.FromBytes(bytes);

            // Act
            var result = JsonNodeModule.From(data);

            // Assert
            await Assert.That(result)
                        .IsSuccess()
                        .WhoseValue
                        .IsEquivalentTo(node);
        });
    }
}

public class JsonNodeModule_From_T_Tests
{
    [Test]
    public async ValueTask Fails_if_the_object_is_null()
    {
        // Act
        var result = JsonNodeModule.From((object?)null);

        // Assert
        await Assert.That(result)
                    .IsError();
    }

    [Test]
    public async ValueTask Roundtrips()
    {
        var gen = JsonObjectGenerator.Any;

        await gen.SampleAsync(async jsonObject =>
        {
            // Act
            var result = JsonNodeModule.From(jsonObject);

            // Assert
            await Assert.That(result)
                        .IsSuccess()
                        .WhoseValue
                        .Satisfies(node => node!.ToJsonString(),
                                   jsonString => jsonString.IsEquivalentTo(jsonObject.ToJsonString()));
        });
    }
}

public class JsonNodeModule_To_T_Tests
{
    [Test]
    public async ValueTask Fails_if_the_node_is_null()
    {
        // Act
        var result = JsonNodeModule.To<JsonObject>(null);

        // Assert
        await Assert.That(result)
                    .IsError();
    }

    [Test]
    public async ValueTask Fails_if_the_node_cannot_be_deserialized_to_target_type()
    {
        var gen =
            from node in JsonNodeGenerator.Any.Null()
            where node is not JsonObject
            select node;

        await gen.SampleAsync(async node =>
        {
            // Act
            var result = JsonNodeModule.To<JsonObject>(node);

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }

    [Test]
    public async ValueTask Roundtrips()
    {
        var gen = JsonObjectGenerator.Any;

        await gen.SampleAsync(async jsonObject =>
        {
            // Arrange
            var node = (JsonNode)jsonObject;

            // Act
            var result = JsonNodeModule.To<JsonObject>(node);

            // Assert
            await Assert.That(result)
                        .IsSuccess()
                        .WhoseValue
                        .IsEquivalentTo(jsonObject);
        });
    }
}

public class JsonNodeModule_Deserialize_T_Tests
{
    [Test]
    public async ValueTask Fails_if_the_binary_data_is_null()
    {
        // Act
        var result = JsonNodeModule.Deserialize<JsonObject>(null);

        // Assert
        await Assert.That(result)
                    .IsError();
    }

    [Test]
    public async ValueTask Fails_if_the_binary_data_cannot_be_deserialized()
    {
        var gen =
            Gen.String
               .Select(BinaryData.FromString)
               .Null();

        await gen.SampleAsync(async data =>
        {
            // Act
            var result = JsonNodeModule.Deserialize<JsonObject>(data);

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }

    [Test]
    public async ValueTask Roundtrips()
    {
        var gen = JsonNodeGenerator.Any;

        await gen.SampleAsync(async node =>
        {
            // Arrange
            var bytes = JsonSerializer.SerializeToUtf8Bytes(node);
            var data = BinaryData.FromBytes(bytes);

            // Act
            var result = JsonNodeModule.Deserialize<JsonNode>(data);

            // Assert
            await Assert.That(result)
                        .IsSuccess()
                        .WhoseValue
                        .IsEquivalentTo(node);
        });
    }
}

public class JsonNodeModule_ToStream_Tests
{
    [Test]
    public async ValueTask Roundtrips()
    {
        var gen = JsonNodeGenerator.Any;

        await gen.SampleAsync(async node =>
        {
            // Act
            using var stream = JsonNodeModule.ToStream(node);
            var deserializedNode = await JsonSerializer.DeserializeAsync<JsonNode>(stream, options: default, Common.CancellationToken);

            // Assert
            await Assert.That(deserializedNode)
                        .IsEquivalentTo(node);
        });
    }
}