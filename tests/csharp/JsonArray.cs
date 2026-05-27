using CsCheck;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using TUnit.Assertions.Enums;

namespace common.tests;

file static class Common
{
    public static CancellationToken CancellationToken =>
        TestContext.Current?.Execution.CancellationToken ?? CancellationToken.None;
}

public class JsonArrayModule_ToJsonArray_Tests
{
    [Test]
    public async ValueTask Roundtrips()
    {
        var gen = JsonNodeGenerator.Any.Null().ArrayOf();

        await gen.SampleAsync(async nodes =>
        {
            // Act
            var array = nodes.ToJsonArray();

            // Assert
            await Assert.That(array.AsEnumerable())
                        .IsEquivalentTo(nodes, CollectionOrdering.Matching);
        });
    }
}

public class JsonArrayModule_ToJsonArray_WithAsyncEnumerable_Tests
{

    [Test]
    public async ValueTask Roundtrips()
    {
        var gen =
            from nodes in JsonNodeGenerator.Any.Null().ArrayOf()
            select nodes.ToAsyncEnumerable();

        await gen.SampleAsync(async nodes =>
        {
            // Act
            var array = await nodes.ToJsonArray(Common.CancellationToken);

            // Assert
            await Assert.That(nodes)
                        .IsEquivalentTo(array, CollectionOrdering.Matching, Common.CancellationToken);
        });
    }
}

public class JsonArrayModule_GetJsonObjects_Tests
{
    [Test]
    public async ValueTask Succeeds_when_all_nodes_are_objects()
    {
        var gen = JsonNodeGenerator.JsonObject.ArrayOf();

        await gen.SampleAsync(async nodes =>
        {
            // Arrange
            var jsonArray = new JsonArray([.. nodes]);

            // Act
            var result = jsonArray.GetJsonObjects();

            // Assert
            await Assert.That(result)
                        .IsSuccess()
                        .WhoseValue
                        .IsEquivalentTo(nodes, CollectionOrdering.Matching);
        });
    }

    [Test]
    public async ValueTask Fails_when_any_node_is_not_an_object()
    {
        var gen =
            from nodes in JsonNodeGenerator.Any.Null().ArrayOf()
            where nodes.Any(node => node is not JsonObject)
            select nodes;

        await gen.SampleAsync(async nodes =>
        {
            // Arrange
            var jsonArray = new JsonArray([.. nodes]);

            // Act
            var result = jsonArray.GetJsonObjects();

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }
}

public class JsonArrayModule_GetElements_Tests
{
    [Test]
    public async ValueTask Succeeds_when_all_nodes_are_valid()
    {
        var gen =
            from nodes in JsonNodeGenerator.Any.Null().ArrayOf()
            from f1 in MapperGenerator.ObjectToObject
            let f = new Func<JsonNode?, Result<object>>(node => Result.Success(f1(node ?? new JsonObject())))
            let g = new Func<int, Error>(index => throw new UnreachableException())
            select (nodes, f, g);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (nodes, selector, errorFromIndex) = tuple;
            var jsonArray = new JsonArray([.. nodes]);

            // Act
            var result = jsonArray.GetElements(selector, errorFromIndex);

            // Assert
            await Assert.That(result).IsSuccess();
        });
    }

    [Test]
    public async ValueTask Fails_when_any_node_is_invalid()
    {
        var gen =
            from nodes in JsonNodeGenerator.Any.Null().ArrayOf()
            from failingNodes in
                Generator.SubSetOf([.. from node in nodes
                                       where node is not null
                                       select node])
            where failingNodes.Count > 0
            let f = new Func<JsonNode?, Result<object>>(node => failingNodes.Contains(node)
                                                                ? Error.From("Node is invalid.")
                                                                : Result.Success((object)Unit.Instance))
            let g = new Func<int, Error>(index => Error.From($"Node at index {index} is invalid."))
            select (nodes, f, g);

        await gen.SampleAsync(async tuple =>
        {
            // Arrange
            var (nodes, selector, errorFromIndex) = tuple;
            var jsonArray = new JsonArray([.. nodes]);

            // Act
            var result = jsonArray.GetElements(selector, errorFromIndex);

            // Assert
            await Assert.That(result).IsError();
        });
    }
}

public class JsonArrayModule_GetJsonArrays_Tests
{
    [Test]
    public async ValueTask Succeeds_when_all_nodes_are_arrays()
    {
        var gen = JsonNodeGenerator.JsonArray.ArrayOf();

        await gen.SampleAsync(async nodes =>
        {
            // Arrange
            var jsonArray = new JsonArray([.. nodes]);

            // Act
            var result = jsonArray.GetJsonArrays();

            // Assert
            await Assert.That(result)
                        .IsSuccess()
                        .WhoseValue
                        .IsEquivalentTo(nodes, CollectionOrdering.Matching);
        });
    }

    [Test]
    public async ValueTask Fails_when_any_node_is_not_an_array()
    {
        var gen =
            from nodes in JsonNodeGenerator.Any.Null().ArrayOf()
            where nodes.Any(node => node is not JsonArray)
            select nodes;

        await gen.SampleAsync(async nodes =>
        {
            // Arrange
            var jsonArray = new JsonArray([.. nodes]);

            // Act
            var result = jsonArray.GetJsonArrays();

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }
}

public class JsonArrayModule_GetJsonValues_Tests
{
    [Test]
    public async ValueTask Succeeds_when_all_nodes_are_values()
    {
        var gen = JsonNodeGenerator.JsonValue.ArrayOf();

        await gen.SampleAsync(async nodes =>
        {
            // Arrange
            var jsonArray = new JsonArray([.. nodes]);

            // Act
            var result = jsonArray.GetJsonValues();

            // Assert
            await Assert.That(result)
                        .IsSuccess()
                        .WhoseValue
                        .IsEquivalentTo(nodes, CollectionOrdering.Matching);
        });
    }

    [Test]
    public async ValueTask Fails_when_any_node_is_not_a_value()
    {
        var gen =
            from nodes in JsonNodeGenerator.Any.Null().ArrayOf()
            where nodes.Any(node => node is not JsonValue)
            select nodes;

        await gen.SampleAsync(async nodes =>
        {
            // Arrange
            var jsonArray = new JsonArray([.. nodes]);

            // Act
            var result = jsonArray.GetJsonValues();

            // Assert
            await Assert.That(result)
                        .IsError();
        });
    }
}