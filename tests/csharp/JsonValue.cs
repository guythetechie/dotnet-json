using CsCheck;
using FluentAssertions;
using System.ComponentModel.Design;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace common.tests;

public class JsonValueTests
{
    [Fact]
    public void AsString_fails_if_the_node_is_null()
    {
        var value = (JsonValue?)null;
        var result = value.AsString();
        result.Should().BeError();
    }

    [Fact]
    public void AsString_fails_if_the_value_is_not_a_string()
    {
        var generator = from value in Generator.JsonValue
                        where value.GetValueKind() != JsonValueKind.String
                        select value;

        generator.Sample(value =>
        {
            var result = value.AsString();
            result.Should().BeError();
        });
    }

    [Fact]
    public void AsString_succeeds_if_the_value_is_a_string()
    {
        var generator = from value in Generator.JsonValue
                        where value.GetValueKind() == JsonValueKind.String
                        select value;
        generator.Sample(value =>
        {
            var result = value.AsString();
            result.Should().BeSuccess();
        });
    }

    [Fact]
    public void AsInt_fails_if_the_node_is_null()
    {
        var value = (JsonValue?)null;
        var result = value.AsInt();
        result.Should().BeError();
    }

    [Fact]
    public void AsInt_fails_if_the_value_is_not_an_int()
    {
        var generator = Gen.OneOf(from value in Gen.String
                                  select JsonValue.Create(value),
                                  from value in Gen.Bool
                                  select JsonValue.Create(value),
                                  from value in Gen.Char
                                  select JsonValue.Create(value),
                                  from value in Gen.Decimal
                                  where decimal.IsInteger(value) is false
                                  select JsonValue.Create(value),
                                  from value in Gen.Double
                                  where double.IsNaN(value) is false
                                  where double.IsInfinity(value) is false
                                  where double.IsInteger(value) is false
                                  select JsonValue.Create(value));

        generator.Sample(value =>
        {
            var result = value.AsInt();
            result.Should().BeError();
        });
    }

    [Fact]
    public void AsInt_succeeds_if_the_value_is_an_int()
    {
        var generator = from value in Gen.Int
                        select JsonValue.Create(value);

        generator.Sample(value =>
        {
            var result = value.AsInt();
            result.Should().BeSuccess();
        });
    }
}
