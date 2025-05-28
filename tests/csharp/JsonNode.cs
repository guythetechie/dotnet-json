using CsCheck;
using System;
using System.Text.Json.Nodes;
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
