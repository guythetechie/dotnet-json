module JsonObjectTests

open Xunit
open common
open Faqt
open FsCheck
open FsCheck.FSharp
open System
open System.Text.Json.Nodes

let private propertyNameGen =
    Gen.generateDefault<NonWhiteSpaceString> () |> Gen.map (fun value -> value.Get)

let private generateJsonObjectWithProperty propertyValueGen =
    gen {
        let! propertyName = propertyNameGen
        let! propertyValue = propertyValueGen
        let! jsonObject = Gen.jsonObject |> Gen.map (JsonObject.setProperty propertyName propertyValue)

        return
            {| PropertyName = propertyName
               PropertyValue = propertyValue
               JsonObject = jsonObject |}
    }

let nodesAreEqual (first: #JsonNode) (second: #JsonNode) =
    String.Equals(first.ToJsonString(), second.ToJsonString(), StringComparison.OrdinalIgnoreCase)

let nodeEquals node (value: obj) =
    nodesAreEqual node (JsonValue.Create value)

[<Fact>]
let ``getProperty fails if the property has a null value`` () =
    let gen = generateJsonObjectWithProperty (Gen.constant null)

    Check.fromGen gen (fun x ->
        let propertyName, jsonObject = x.PropertyName, x.JsonObject
        let result = JsonObject.getProperty propertyName jsonObject
        result.Should().BeFailure())

[<Fact>]
let ``getProperty fails if the property is missing`` () =
    let gen =
        gen {
            let! jsonObject = Gen.jsonObject
            let! propertyName = propertyNameGen |> Gen.filter (jsonObject.ContainsKey >> not)
            return propertyName, jsonObject
        }

    Check.fromGen gen (fun (propertyName, jsonObject) ->
        let result = JsonObject.getProperty propertyName jsonObject
        result.Should().BeFailure())

[<Fact>]
let ``getProperty succeeds if the property exists`` () =
    let gen = generateJsonObjectWithProperty Gen.jsonNode

    Check.fromGen gen (fun x ->
        let propertyName, propertyValue, jsonObject =
            x.PropertyName, x.PropertyValue, x.JsonObject

        let result = JsonObject.getProperty propertyName jsonObject

        result.Should().BeSuccess().That.Should().Be(propertyValue, nodesAreEqual))

[<Fact>]
let ``getOptionalProperty return None if the property has a null value`` () =
    let gen = generateJsonObjectWithProperty (Gen.constant null)

    Check.fromGen gen (fun x ->
        let propertyName, jsonObject = x.PropertyName, x.JsonObject
        let result = JsonObject.getOptionalProperty propertyName jsonObject
        result.Should().BeNone())

[<Fact>]
let ``getOptionalProperty return None if the property is missing`` () =
    let gen =
        gen {
            let! jsonObject = Gen.jsonObject
            let! propertyName = propertyNameGen |> Gen.filter (jsonObject.ContainsKey >> not)
            return propertyName, jsonObject
        }

    Check.fromGen gen (fun (propertyName, jsonObject) ->
        let result = JsonObject.getOptionalProperty propertyName jsonObject
        result.Should().BeNone())

[<Fact>]
let ``getOptionalProperty return Some if the property exists`` () =
    let gen = generateJsonObjectWithProperty Gen.jsonNode

    Check.fromGen gen (fun x ->
        let propertyName, propertyValue, jsonObject =
            x.PropertyName, x.PropertyValue, x.JsonObject

        let result = JsonObject.getOptionalProperty propertyName jsonObject

        result.Should().BeSome().That.Should().Be(propertyValue, nodesAreEqual))

[<Fact>]
let ``getJsonObjectProperty fails if the property is not a JSON object`` () =
    let gen = generateJsonObjectWithProperty Gen.JsonNode.nonJsonObject

    Check.fromGen gen (fun x ->
        let propertyName, jsonObject = x.PropertyName, x.JsonObject
        let result = JsonObject.getJsonObjectProperty propertyName jsonObject
        result.Should().BeFailure())

[<Fact>]
let ``getJsonObjectProperty succeeds if the property is a JSON object`` () =
    let gen = generateJsonObjectWithProperty Gen.JsonNode.jsonObject

    Check.fromGen gen (fun x ->
        let propertyName, propertyValue, jsonObject =
            x.PropertyName, x.PropertyValue, x.JsonObject

        let result = JsonObject.getJsonObjectProperty propertyName jsonObject
        result.Should().BeSuccess().That.Should().Be(propertyValue, nodesAreEqual))

[<Fact>]
let ``getJsonArrayProperty fails if the property is not a JSON array`` () =
    let gen = generateJsonObjectWithProperty Gen.JsonNode.nonJsonArray

    Check.fromGen gen (fun x ->
        let propertyName, jsonObject = x.PropertyName, x.JsonObject
        let result = JsonObject.getJsonArrayProperty propertyName jsonObject
        result.Should().BeFailure())

[<Fact>]
let ``getJsonArrayProperty succeeds if the property is a JSON array`` () =
    let gen = generateJsonObjectWithProperty Gen.JsonNode.jsonArray

    Check.fromGen gen (fun x ->
        let propertyName, propertyValue, jsonObject =
            x.PropertyName, x.PropertyValue, x.JsonObject

        let result = JsonObject.getJsonArrayProperty propertyName jsonObject
        result.Should().BeSuccess().That.Should().Be(propertyValue, nodesAreEqual))

[<Fact>]
let ``getJsonValueProperty fails if the property is not a JSON value`` () =
    let gen = generateJsonObjectWithProperty Gen.JsonNode.nonJsonValue

    Check.fromGen gen (fun x ->
        let propertyName, jsonObject = x.PropertyName, x.JsonObject
        let result = JsonObject.getJsonValueProperty propertyName jsonObject
        result.Should().BeFailure())

[<Fact>]
let ``getJsonValue succeeds if the property is a JSON value`` () =
    let gen = generateJsonObjectWithProperty Gen.JsonNode.jsonValue

    Check.fromGen gen (fun x ->
        let propertyName, propertyValue, jsonObject =
            x.PropertyName, x.PropertyValue, x.JsonObject

        let result = JsonObject.getJsonValueProperty propertyName jsonObject
        result.Should().BeSuccess().That.Should().Be(propertyValue, nodesAreEqual))

[<Fact>]
let ``getStringProperty fails if the property is not a string`` () =
    let gen = generateJsonObjectWithProperty Gen.JsonValue.nonString

    Check.fromGen gen (fun x ->
        let propertyName, jsonObject = x.PropertyName, x.JsonObject
        let result = JsonObject.getStringProperty propertyName jsonObject
        result.Should().BeFailure())

[<Fact>]
let ``getStringProperty succeeds if the property is a string`` () =
    let gen = generateJsonObjectWithProperty Gen.JsonValue.string

    Check.fromGen gen (fun x ->
        let propertyName, propertyValue, jsonObject =
            x.PropertyName, x.PropertyValue, x.JsonObject

        let result = JsonObject.getStringProperty propertyName jsonObject

        result.Should().BeSuccess().That.Should().Satisfy(fun value -> nodeEquals propertyValue value))

[<Fact>]
let ``getAbsoluteUriProperty fails if the property is not an absolute URI`` () =
    let gen = generateJsonObjectWithProperty Gen.JsonValue.nonAbsoluteUri

    Check.fromGen gen (fun x ->
        let propertyName, jsonObject = x.PropertyName, x.JsonObject
        let result = JsonObject.getAbsoluteUriProperty propertyName jsonObject
        result.Should().BeFailure())

[<Fact>]
let ``getAbsoluteUriProperty succeeds if the property is an absolute URI`` () =
    let gen = generateJsonObjectWithProperty Gen.JsonValue.absoluteUri

    Check.fromGen gen (fun x ->
        let propertyName, propertyValue, jsonObject =
            x.PropertyName, x.PropertyValue, x.JsonObject

        let result = JsonObject.getAbsoluteUriProperty propertyName jsonObject

        result.Should().BeSuccess().That.Should().Satisfy(fun value -> nodeEquals propertyValue value))

[<Fact>]
let ``getGuidProperty fails if the property is not a GUID`` () =
    let gen = generateJsonObjectWithProperty Gen.JsonValue.nonGuid

    Check.fromGen gen (fun x ->
        let propertyName, jsonObject = x.PropertyName, x.JsonObject
        let result = JsonObject.getGuidProperty propertyName jsonObject
        result.Should().BeFailure())

[<Fact>]
let ``getGuidProperty succeeds if the property is a GUID`` () =
    let gen = generateJsonObjectWithProperty Gen.JsonValue.guid

    Check.fromGen gen (fun x ->
        let propertyName, propertyValue, jsonObject =
            x.PropertyName, x.PropertyValue, x.JsonObject

        let result = JsonObject.getGuidProperty propertyName jsonObject

        result.Should().BeSuccess().That.Should().Satisfy(fun value -> nodeEquals propertyValue value))

[<Fact>]
let ``getBoolProperty fails if the property is not a boolean`` () =
    let gen = generateJsonObjectWithProperty Gen.JsonValue.nonBool

    Check.fromGen gen (fun x ->
        let propertyName, jsonObject = x.PropertyName, x.JsonObject
        let result = JsonObject.getBoolProperty propertyName jsonObject
        result.Should().BeFailure())

[<Fact>]
let ``getBoolProperty succeeds if the property is a boolean`` () =
    let gen = generateJsonObjectWithProperty Gen.JsonValue.bool

    Check.fromGen gen (fun x ->
        let propertyName, propertyValue, jsonObject =
            x.PropertyName, x.PropertyValue, x.JsonObject

        let result = JsonObject.getBoolProperty propertyName jsonObject

        result.Should().BeSuccess().That.Should().Satisfy(fun value -> nodeEquals propertyValue value))

[<Fact>]
let ``getIntProperty fails if the property is not an integer`` () =
    let gen = generateJsonObjectWithProperty Gen.JsonValue.nonInteger

    Check.fromGen gen (fun x ->
        let propertyName, jsonObject = x.PropertyName, x.JsonObject
        let result = JsonObject.getIntProperty propertyName jsonObject
        result.Should().BeFailure())

[<Fact>]
let ``getIntProperty succeeds if the property is an integer`` () =
    let gen = generateJsonObjectWithProperty Gen.JsonValue.integer

    Check.fromGen gen (fun x ->
        let propertyName, propertyValue, jsonObject =
            x.PropertyName, x.PropertyValue, x.JsonObject

        let result = JsonObject.getIntProperty propertyName jsonObject

        result.Should().BeSuccess().That.Should().Satisfy(fun value -> nodeEquals propertyValue value))

[<Fact>]
let ``setProperty sets the property value`` () =
    let gen =
        gen {
            let! propertyName = propertyNameGen
            let! propertyValue = Gen.jsonNode
            let! jsonObject = Gen.jsonObject

            return propertyName, propertyValue, jsonObject
        }

    Check.fromGen gen (fun (propertyName, propertyValue, jsonObject) ->
        let updatedJsonObject = JsonObject.setProperty propertyName propertyValue jsonObject
        updatedJsonObject.Should().ContainKey(propertyName).WhoseValue.Value.Should().Be(propertyValue, nodesAreEqual))
