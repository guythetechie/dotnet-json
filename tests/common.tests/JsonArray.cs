using CsCheck;
using FluentAssertions;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace common.tests;

public class JsonArrayTests
{
    [Fact]
    public void ToJsonArray_has_correct_count()
    {
        var generator = from count in Gen.Int[0, 100]
                        from nodes in Generator.JsonNode.Null().Array[count]
                        select (count, nodes);

        generator.Sample((count, nodes) =>
        {
            var jsonArray = nodes.ToJsonArray();
            jsonArray.AsEnumerable().Should().HaveCount(count);
        });
    }

    [Fact]
    public async Task ToJsonArray_with_async_enumerable_has_correct_count()
    {
        var generator = from count in Gen.Int[0, 100]
                        from nodes in Generator.JsonNode.Null().Array[count]
                        select (count, nodes.ToAsyncEnumerable());

        await generator.SampleAsync(async (count, nodes) =>
        {
            var jsonArray = await nodes.ToJsonArray(CancellationToken.None);
            jsonArray.AsEnumerable().Should().HaveCount(count);
        });
    }

    [Fact]
    public void GetJsonObjects_fails_if_the_array_contains_a_non_object_node()
    {
        var generator = from jsonArray in Generator.JsonArray
                        where jsonArray.Any(node => node is not JsonObject)
                        select jsonArray;

        generator.Sample(jsonArray =>
        {
            var result = jsonArray.GetJsonObjects();
            result.Should().BeError();
        });
    }

    [Fact]
    public void GetJsonObjects_succeeds_if_the_array_is_empty()
    {
        var jsonArray = new JsonArray();
        var result = jsonArray.GetJsonObjects();
        result.Should().BeSuccess();
    }

    [Fact]
    public void GetJsonObjects_succeeds_if_the_array_only_contains_object_nodes()
    {
        var generator = from jsonObjects in Generator.JsonObject.Array[0, 10]
                        where jsonObjects.Length > 0
                        select new JsonArray(jsonObjects);

        generator.Sample(jsonArray =>
        {
            var result = jsonArray.GetJsonObjects();
            result.Should().BeSuccess();
        });
    }

    [Fact]
    public void GetJsonArrays_fails_if_the_array_contains_a_non_array_node()
    {
        var generator = from jsonArray in Generator.JsonArray
                        where jsonArray.Any(node => node is not JsonArray)
                        select jsonArray;

        generator.Sample(jsonArray =>
        {
            var result = jsonArray.GetJsonArrays();
            result.Should().BeError();
        });
    }

    [Fact]
    public void GetJsonArrays_succeeds_if_the_array_is_empty()
    {
        var jsonArray = new JsonArray();
        var result = jsonArray.GetJsonArrays();
        result.Should().BeSuccess();
    }

    [Fact]
    public void GetJsonArrays_succeeds_if_the_array_only_contains_array_nodes()
    {
        var generator = from jsonArrays in Generator.JsonArray.Array[0, 10]
                        where jsonArrays.Length > 0
                        select new JsonArray(jsonArrays);

        generator.Sample(jsonArray =>
        {
            var result = jsonArray.GetJsonArrays();
            result.Should().BeSuccess();
        });
    }

    [Fact]
    public void GetJsonValues_fails_if_the_array_contains_a_non_value_node()
    {
        var generator = from jsonArray in Generator.JsonArray
                        where jsonArray.Any(node => node is not JsonValue)
                        select jsonArray;

        generator.Sample(jsonArray =>
        {
            var result = jsonArray.GetJsonValues();
            result.Should().BeError();
        });
    }

    [Fact]
    public void GetJsonValues_succeeds_if_the_array_is_empty()
    {
        var jsonArray = new JsonArray();
        var result = jsonArray.GetJsonValues();
        result.Should().BeSuccess();
    }

    [Fact]
    public void GetJsonValues_succeeds_if_the_array_only_contains_value_nodes()
    {
        var generator = from jsonValues in Generator.JsonValue.Array[0, 10]
                        where jsonValues.Length > 0
                        select new JsonArray(jsonValues);

        generator.Sample(jsonArray =>
        {
            var result = jsonArray.GetJsonValues();
            result.Should().BeSuccess();
        });
    }
}
