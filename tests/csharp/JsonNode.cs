using CsCheck;
using FluentAssertions;
using System;
using System.IO;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace common.tests;

public class JsonNodeTests
{
    [Fact]
    public void AsJsonObject_fails_if_the_node_is_null()
    {
        var node = (JsonNode?)null;
        var result = node.AsJsonObject();
        result.Should().BeError();
    }

    [Fact]
    public void AsJsonObject_fails_if_the_node_is_not_a_json_object()
    {
        var generator = JsonNodeGenerator.NonJsonObject;

        generator.Sample(node =>
        {
            var result = node.AsJsonObject();
            result.Should().BeError();
        });
    }

    [Fact]
    public void AsJsonObject_succeeds_if_the_node_is_a_json_object()
    {
        var generator = JsonNodeGenerator.JsonObject;

        generator.Sample(node =>
        {
            var result = node.AsJsonObject();
            result.Should().BeSuccess().Which.Should().BeAssignableTo<JsonObject>();
        });
    }

    [Fact]
    public void AsJsonArray_fails_if_the_node_is_null()
    {
        var node = (JsonNode?)null;
        var result = node.AsJsonArray();
        result.Should().BeError();
    }

    [Fact]
    public void AsJsonArray_fails_if_the_node_is_not_a_json_array()
    {
        var generator = JsonNodeGenerator.NonJsonArray;
        generator.Sample(node =>
        {
            var result = node.AsJsonArray();
            result.Should().BeError();
        });
    }

    [Fact]
    public void AsJsonArray_succeeds_if_the_node_is_a_json_array()
    {
        var generator = JsonNodeGenerator.JsonArray;
        generator.Sample(node =>
        {
            var result = node.AsJsonArray();
            result.Should().BeSuccess().Which.Should().BeAssignableTo<JsonArray>();
        });
    }

    [Fact]
    public void AsJsonValue_fails_if_the_node_is_null()
    {
        var node = (JsonNode?)null;
        var result = node.AsJsonValue();
        result.Should().BeError();
    }

    [Fact]
    public void AsJsonValue_fails_if_the_node_is_not_a_json_value()
    {
        var generator = JsonNodeGenerator.NonJsonValue;

        generator.Sample(node =>
        {
            var result = node.AsJsonValue();
            result.Should().BeError();
        });
    }

    [Fact]
    public void AsJsonValue_succeeds_if_the_node_is_a_json_value()
    {
        var generator = JsonNodeGenerator.JsonValue;

        generator.Sample(node =>
        {
            var result = node.AsJsonValue();
            result.Should().BeSuccess().Which.Should().BeAssignableTo<JsonValue>();
        });
    }

    [Fact]
    public async Task From_with_stream_fails_if_the_stream_is_null()
    {
        var stream = (Stream?)null;
        var result = await JsonNodeModule.From(stream, default, default, CancellationToken.None);
        result.Should().BeError();
    }

    [Fact]
    public async Task From_with_stream_fails_if_the_stream_contains_invalid_json()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("invalid json"));
        var result = await JsonNodeModule.From(stream, default, default, CancellationToken.None);
        result.Should().BeError();
    }

    [Fact]
    public void From_with_stream_succeeds_if_the_stream_contains_valid_json()
    {
        var generator = JsonNodeGenerator.Value;

        generator.Sample(node =>
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(node.ToJsonString()));
            var task = JsonNodeModule.From(stream, default, default, CancellationToken.None);
            var result = task.Result;
            result.Should().BeSuccess().Which.Should().BeEquivalentTo(node);
        });
    }

    [Fact]
    public void From_with_binary_data_fails_if_the_data_is_null()
    {
        var data = (BinaryData?)null;
        var result = JsonNodeModule.From(data);
        result.Should().BeError();
    }

    [Fact]
    public void From_with_binary_data_fails_if_the_data_contains_invalid_json()
    {
        var data = BinaryData.FromString("invalid json");
        var result = JsonNodeModule.From(data);
        result.Should().BeError();
    }

    [Fact]
    public void From_with_binary_data_succeeds_if_the_data_contains_valid_json()
    {
        var generator = JsonNodeGenerator.Value;

        generator.Sample(node =>
        {
            var data = BinaryData.FromString(node.ToJsonString());
            var result = JsonNodeModule.From(data);
            result.Should().BeSuccess().Which.Should().BeEquivalentTo(node);
        });
    }

    [Fact]
    public void From_with_object_succeeds_with_serializable_object()
    {
        var testObject = new { Name = "Test", Value = 42 };
        var result = JsonNodeModule.From(testObject);
        result.Should().BeSuccess().Which.Should().BeAssignableTo<JsonNode>();
    }

    [Fact]
    public void From_with_object_fails_if_the_object_is_null()
    {
        var result = JsonNodeModule.From((object?)null);
        result.Should().BeError();
    }

    [Fact]
    public void To_fails_if_the_node_is_null()
    {
        var node = (JsonNode?)null;
        var result = JsonNodeModule.To<JsonObject>(node);
        result.Should().BeError();
    }

    [Fact]
    public void To_fails_if_the_node_cannot_be_deserialized_to_target_type()
    {
        var node = JsonNode.Parse("[1, 2, 3]");
        var result = JsonNodeModule.To<JsonObject>(node);
        result.Should().BeError();
    }

    [Fact]
    public void To_succeeds_if_the_node_can_be_deserialized_to_target_type()
    {
        var generator = JsonNodeGenerator.JsonObject;

        generator.Sample(node =>
        {
            var result = JsonNodeModule.To<JsonObject>(node);
            result.Should().BeSuccess().Which.Should().BeAssignableTo<JsonObject>();
        });
    }

    [Fact]
    public void ToStream_creates_stream_from_json_node()
    {
        var generator = JsonNodeGenerator.Value;

        generator.Sample(node =>
        {
            var stream = JsonNodeModule.ToStream(node);
            stream.Should().NotBeNull();

            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();
            content.Should().NotBeEmpty();
        });
    }

    [Fact]
    public void Deserialize_fails_if_the_data_is_null()
    {
        var data = (BinaryData?)null;
        var result = JsonNodeModule.Deserialize<JsonObject>(data);
        result.Should().BeError();
    }

    [Fact]
    public void Deserialize_fails_if_the_data_cannot_be_deserialized()
    {
        var generator = JsonNodeGenerator.NonJsonObject;

        generator.Sample(node =>
        {
            var data = BinaryData.FromString(node.ToJsonString());
            var result = JsonNodeModule.Deserialize<JsonObject>(data);
            result.Should().BeError();
        });
    }

    [Fact]
    public void Deserialize_succeeds_if_the_data_can_be_deserialized()
    {
        var generator = JsonNodeGenerator.Value;

        generator.Sample(node =>
        {
            var data = BinaryData.FromString(node.ToJsonString());
            var result = JsonNodeModule.Deserialize<JsonNode>(data);
            result.Should().BeSuccess().Which.Should().BeEquivalentTo(node);
        });
    }
}
