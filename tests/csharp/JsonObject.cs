using CsCheck;
using FluentAssertions;
using LanguageExt;
using System;
using System.Linq;
using System.Text.Json.Nodes;
using Xunit;

namespace common.tests;

public class JsonObjectTests
{
    [Fact]
    public void GetProperty_fails_if_the_value_is_null()
    {
        var generator = GenerateJsonObject(null);

        generator.Sample(x =>
        {
            var (propertyName, jsonObject) = x;
            var result = jsonObject.GetProperty(propertyName);
            result.Should().BeError();
        });
    }

    private static Gen<(string PropertyName, JsonObject JsonObject)> GenerateJsonObject(JsonNode? propertyValue) =>
        from jsonObject in JsonObjectGenerator.Value
        from propertyName in GeneratePropertyName()
        let updatedJsonObject = jsonObject.SetProperty(propertyName, propertyValue)
        select (propertyName, jsonObject);


    private static Gen<string> GeneratePropertyName() =>
        Gen.String.AlphaNumeric.Where(x => string.IsNullOrWhiteSpace(x) is false);

    [Fact]
    public void GetProperty_fails_if_the_property_is_missing()
    {
        var generator = from jsonObject in JsonObjectGenerator.Value
                        from propertyName in GeneratePropertyName()
                        where jsonObject.ContainsKey(propertyName) is false
                        select (propertyName, jsonObject);

        generator.Sample(x =>
        {
            var (propertyName, jsonObject) = x;
            var result = jsonObject.GetProperty(propertyName);
            result.Should().BeError();
        });
    }

    [Fact]
    public void GetProperty_succeeds_if_the_property_is_present()
    {
        var generator = from propertyValue in JsonNodeGenerator.Value
                        from x in GenerateJsonObject(propertyValue)
                        select (x.PropertyName, propertyValue, x.JsonObject);

        generator.Sample(x =>
        {
            var (propertyName, propertyValue, jsonObject) = x;
            var result = jsonObject.GetProperty(propertyName);
            result.Should().BeSuccess().Which.Should().BeEquivalentTo(propertyValue);
        });
    }

    [Fact]
    public void GetOptionalProperty_returns_none_if_the_property_is_missing()
    {
        var generator = from jsonObject in JsonObjectGenerator.Value
                        from propertyName in GeneratePropertyName()
                        where jsonObject.ContainsKey(propertyName) is false
                        select (propertyName, jsonObject);

        generator.Sample(x =>
        {
            var (propertyName, jsonObject) = x;
            var result = jsonObject.GetOptionalProperty(propertyName);
            result.Should().BeNone();
        });
    }

    [Fact]
    public void GetOptionalProperty_returns_none_if_the_property_is_null()
    {
        var generator = from propertyValue in Gen.Const((JsonNode?)null)
                        from x in GenerateJsonObject(propertyValue)
                        select (x.PropertyName, x.JsonObject);

        generator.Sample(x =>
        {
            var (propertyName, jsonObject) = x;
            var result = jsonObject.GetOptionalProperty(propertyName);
            result.Should().BeNone();
        });
    }

    [Fact]
    public void GetOptionalProperty_returns_some_if_the_property_is_present()
    {
        var generator = from propertyValue in JsonNodeGenerator.Value
                        from x in GenerateJsonObject(propertyValue)
                        select (x.PropertyName, propertyValue, x.JsonObject);

        generator.Sample(x =>
        {
            var (propertyName, propertyValue, jsonObject) = x;
            var result = jsonObject.GetOptionalProperty(propertyName);
            result.Should().BeSome().Which.Should().BeEquivalentTo(propertyValue);
        });
    }

    [Fact]
    public void GetJsonObjectProperty_fails_if_the_property_is_missing()
    {
        var generator = from jsonObject in JsonObjectGenerator.Value
                        from propertyName in GeneratePropertyName()
                        where jsonObject.ContainsKey(propertyName) is false
                        select (propertyName, jsonObject);

        generator.Sample(x =>
        {
            var (propertyName, jsonObject) = x;
            var result = jsonObject.GetJsonObjectProperty(propertyName);
            result.Should().BeError();
        });
    }

    [Fact]
    public void GetJsonObjectProperty_fails_if_the_property_is_not_a_json_object()
    {
        var generator = from propertyValue in JsonNodeGenerator.NonJsonObject
                        from x in GenerateJsonObject(propertyValue)
                        select (x.PropertyName, x.JsonObject);

        generator.Sample(x =>
        {
            var (propertyName, jsonObject) = x;
            var result = jsonObject.GetJsonObjectProperty(propertyName);
            result.Should().BeError();
        });
    }

    [Fact]
    public void GetJsonObjectProperty_succeeds_if_the_property_is_a_json_object()
    {
        var generator = from propertyValue in JsonNodeGenerator.JsonObject
                        from x in GenerateJsonObject(propertyValue)
                        select (x.PropertyName, propertyValue, x.JsonObject);

        generator.Sample(x =>
        {
            var (propertyName, propertyValue, jsonObject) = x;
            var result = jsonObject.GetJsonObjectProperty(propertyName);
            result.Should().BeSuccess().Which.Should().BeEquivalentTo(propertyValue);
        });
    }

    [Fact]
    public void GetJsonArrayProperty_fails_if_the_property_is_missing()
    {
        var generator = from jsonObject in JsonObjectGenerator.Value
                        from propertyName in GeneratePropertyName()
                        where jsonObject.ContainsKey(propertyName) is false
                        select (propertyName, jsonObject);

        generator.Sample(x =>
        {
            var (propertyName, jsonObject) = x;
            var result = jsonObject.GetJsonArrayProperty(propertyName);
            result.Should().BeError();
        });
    }

    [Fact]
    public void GetJsonArrayProperty_fails_if_the_property_is_not_a_json_array()
    {
        var generator = from propertyValue in JsonNodeGenerator.NonJsonArray
                        from x in GenerateJsonObject(propertyValue)
                        select (x.PropertyName, x.JsonObject);

        generator.Sample(x =>
        {
            var (propertyName, jsonObject) = x;
            var result = jsonObject.GetJsonArrayProperty(propertyName);
            result.Should().BeError();
        });
    }

    [Fact]
    public void GetJsonArrayProperty_succeeds_if_the_property_is_a_json_array()
    {
        var generator = from propertyValue in JsonNodeGenerator.JsonArray
                        from x in GenerateJsonObject(propertyValue)
                        select (x.PropertyName, propertyValue, x.JsonObject);

        generator.Sample(x =>
        {
            var (propertyName, propertyValue, jsonObject) = x;
            var result = jsonObject.GetJsonArrayProperty(propertyName);
            result.Should().BeSuccess().Which.Should().BeEquivalentTo(propertyValue);
        });
    }

    [Fact]
    public void GetJsonValueProperty_fails_if_the_property_is_missing()
    {
        var generator = from jsonObject in JsonObjectGenerator.Value
                        from propertyName in GeneratePropertyName()
                        where jsonObject.ContainsKey(propertyName) is false
                        select (propertyName, jsonObject);

        generator.Sample(x =>
        {
            var (propertyName, jsonObject) = x;
            var result = jsonObject.GetJsonValueProperty(propertyName);
            result.Should().BeError();
        });
    }

    [Fact]
    public void GetJsonValueProperty_fails_if_the_property_is_not_a_json_value()
    {
        var generator = from propertyValue in JsonNodeGenerator.NonJsonValue
                        from x in GenerateJsonObject(propertyValue)
                        select (x.PropertyName, x.JsonObject);

        generator.Sample(x =>
        {
            var (propertyName, jsonObject) = x;
            var result = jsonObject.GetJsonValueProperty(propertyName);
            result.Should().BeError();
        });
    }

    [Fact]
    public void GetJsonValueProperty_succeeds_if_the_property_is_a_json_value()
    {
        var generator = from propertyValue in JsonNodeGenerator.JsonValue
                        from x in GenerateJsonObject(propertyValue)
                        select (x.PropertyName, propertyValue, x.JsonObject);

        generator.Sample(x =>
        {
            var (propertyName, propertyValue, jsonObject) = x;
            var result = jsonObject.GetJsonValueProperty(propertyName);
            result.Should().BeSuccess().Which.Should().BeEquivalentTo(propertyValue);
        });
    }

    [Fact]
    public void GetStringProperty_fails_if_the_property_is_missing()
    {
        var generator = from jsonObject in JsonObjectGenerator.Value
                        from propertyName in GeneratePropertyName()
                        where jsonObject.ContainsKey(propertyName) is false
                        select (propertyName, jsonObject);

        generator.Sample(x =>
        {
            var (propertyName, jsonObject) = x;
            var result = jsonObject.GetStringProperty(propertyName);
            result.Should().BeError();
        });
    }

    [Fact]
    public void GetStringProperty_fails_if_the_property_is_not_a_string()
    {
        var generator = from propertyValue in JsonValueGenerator.NonString
                        from x in GenerateJsonObject(propertyValue)
                        select (x.PropertyName, x.JsonObject);

        generator.Sample(x =>
        {
            var (propertyName, jsonObject) = x;
            var result = jsonObject.GetStringProperty(propertyName);
            result.Should().BeError();
        });
    }

    [Fact]
    public void GetStringProperty_succeeds_if_the_property_is_a_string()
    {
        var generator = from propertyValue in JsonValueGenerator.String
                        from x in GenerateJsonObject(propertyValue)
                        select (x.PropertyName, propertyValue, x.JsonObject);

        generator.Sample(x =>
        {
            var (propertyName, propertyValue, jsonObject) = x;
            var result = jsonObject.GetStringProperty(propertyName);
            result.Should().BeSuccess().Which.Should().Be(propertyValue.GetValue<string>());
        });
    }

    [Fact]
    public void GetIntProperty_fails_if_the_property_is_missing()
    {
        var generator = from jsonObject in JsonObjectGenerator.Value
                        from propertyName in GeneratePropertyName()
                        where jsonObject.ContainsKey(propertyName) is false
                        select (propertyName, jsonObject);

        generator.Sample(x =>
        {
            var (propertyName, jsonObject) = x;
            var result = jsonObject.GetIntProperty(propertyName);
            result.Should().BeError();
        });
    }

    [Fact]
    public void GetIntProperty_fails_if_the_property_is_not_an_int()
    {
        var generator = from propertyValue in JsonValueGenerator.NonInt
                        from x in GenerateJsonObject(propertyValue)
                        select (x.PropertyName, x.JsonObject);

        generator.Sample(x =>
        {
            var (propertyName, jsonObject) = x;
            var result = jsonObject.GetIntProperty(propertyName);
            result.Should().BeError();
        });
    }

    [Fact]
    public void GetIntProperty_succeeds_if_the_property_is_an_int()
    {
        var generator = from propertyValue in JsonValueGenerator.Int
                        from x in GenerateJsonObject(propertyValue)
                        select (x.PropertyName, propertyValue, x.JsonObject);
        generator.Sample(x =>
        {
            var (propertyName, propertyValue, jsonObject) = x;
            var result = jsonObject.GetIntProperty(propertyName);
            result.Should().BeSuccess().Which.Should().Be(propertyValue.GetValue<int>());
        });
    }

    [Fact]
    public void GetBoolProperty_fails_if_the_property_is_missing()
    {
        var generator = from jsonObject in JsonObjectGenerator.Value
                        from propertyName in GeneratePropertyName()
                        where jsonObject.ContainsKey(propertyName) is false
                        select (propertyName, jsonObject);

        generator.Sample(x =>
        {
            var (propertyName, jsonObject) = x;
            var result = jsonObject.GetBoolProperty(propertyName);
            result.Should().BeError();
        });
    }

    [Fact]
    public void GetBoolProperty_fails_if_the_property_is_not_a_bool()
    {
        var generator = from propertyValue in JsonValueGenerator.NonBool
                        from x in GenerateJsonObject(propertyValue)
                        select (x.PropertyName, x.JsonObject);

        generator.Sample(x =>
        {
            var (propertyName, jsonObject) = x;
            var result = jsonObject.GetBoolProperty(propertyName);
            result.Should().BeError();
        });
    }

    [Fact]
    public void GetBoolProperty_succeeds_if_the_property_is_a_bool()
    {
        var generator = from propertyValue in JsonValueGenerator.Bool
                        from x in GenerateJsonObject(propertyValue)
                        select (x.PropertyName, propertyValue, x.JsonObject);
        generator.Sample(x =>
        {
            var (propertyName, propertyValue, jsonObject) = x;
            var result = jsonObject.GetBoolProperty(propertyName);
            result.Should().BeSuccess().Which.Should().Be(propertyValue.GetValue<bool>());
        });
    }

    [Fact]
    public void GetGuidProperty_fails_if_the_property_is_missing()
    {
        var generator = from jsonObject in JsonObjectGenerator.Value
                        from propertyName in GeneratePropertyName()
                        where jsonObject.ContainsKey(propertyName) is false
                        select (propertyName, jsonObject);

        generator.Sample(x =>
        {
            var (propertyName, jsonObject) = x;
            var result = jsonObject.GetGuidProperty(propertyName);
            result.Should().BeError();
        });
    }

    [Fact]
    public void GetGuidProperty_fails_if_the_property_is_not_a_guid()
    {
        var generator = from propertyValue in JsonValueGenerator.NonGuid
                        from x in GenerateJsonObject(propertyValue)
                        select (x.PropertyName, x.JsonObject);

        generator.Sample(x =>
        {
            var (propertyName, jsonObject) = x;
            var result = jsonObject.GetGuidProperty(propertyName);
            result.Should().BeError();
        });
    }

    [Fact]
    public void GetGuidProperty_succeeds_if_the_property_is_a_guid()
    {
        var generator = from propertyValue in JsonValueGenerator.Guid
                        from x in GenerateJsonObject(propertyValue)
                        select (x.PropertyName, propertyValue, x.JsonObject);

        generator.Sample(x =>
        {
            var (propertyName, propertyValue, jsonObject) = x;
            var result = jsonObject.GetGuidProperty(propertyName);
            result.Should().BeSuccess().Which.Should().Be(propertyValue.GetValue<Guid>());
        });
    }

    [Fact]
    public void GetAbsoluteUriProperty_fails_if_the_property_is_missing()
    {
        var generator = from jsonObject in JsonObjectGenerator.Value
                        from propertyName in GeneratePropertyName()
                        where jsonObject.ContainsKey(propertyName) is false
                        select (propertyName, jsonObject);

        generator.Sample(x =>
        {
            var (propertyName, jsonObject) = x;
            var result = jsonObject.GetAbsoluteUriProperty(propertyName);
            result.Should().BeError();
        });
    }

    [Fact]
    public void GetAbsoluteUriProperty_fails_if_the_property_is_not_an_absolute_uri()
    {
        var generator = from propertyValue in JsonValueGenerator.NonAbsoluteUri
                        from x in GenerateJsonObject(propertyValue)
                        select (x.PropertyName, x.JsonObject);

        generator.Sample(x =>
        {
            var (propertyName, jsonObject) = x;
            var result = jsonObject.GetAbsoluteUriProperty(propertyName);
            result.Should().BeError();
        });
    }

    [Fact]
    public void GetAbsoluteUriProperty_succeeds_if_the_property_is_an_absolute_uri()
    {
        var generator = from propertyValue in JsonValueGenerator.AbsoluteUri
                        from x in GenerateJsonObject(propertyValue)
                        select (x.PropertyName, propertyValue, x.JsonObject);

        generator.Sample(x =>
        {
            var (propertyName, propertyValue, jsonObject) = x;
            var result = jsonObject.GetAbsoluteUriProperty(propertyName);
            result.Should().BeSuccess().Which.Should().Be(propertyValue.GetValue<Uri>());
        });
    }
}
