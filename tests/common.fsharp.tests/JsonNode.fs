module JsonNodeTests

open Xunit
open common.fsharp
open Faqt

[<Fact>]
let ``asJsonObject with null returns a failure`` () =
    let result = JsonNode.asJsonObject null
    result.Should().BeFailure()

[<Fact>]
let ``asJsonObject with non JSON objects returns a failure`` () =
    let gen = Gen.JsonNode.nonJsonObject

    Check.fromGen gen (fun node ->
        let result = JsonNode.asJsonObject node
        result.Should().BeFailure())

[<Fact>]
let ``asJsonObject with JSON objects returns a success`` () =
    let gen = Gen.jsonObject

    Check.fromGen gen (fun node ->
        let result = JsonNode.asJsonObject node
        result.Should().BeSuccess())

[<Fact>]
let ``asJsonArray with null returns a failure`` () =
    let result = JsonNode.asJsonArray null
    result.Should().BeFailure

[<Fact>]
let ``asJsonArray with non JSON arrays returns a failure`` () =
    let gen = Gen.JsonNode.nonJsonArray

    Check.fromGen gen (fun node ->
        let result = JsonNode.asJsonArray node
        result.Should().BeFailure())

[<Fact>]
let ``asJsonArray with JSON arrays returns a success`` () =
    let gen = Gen.jsonArray

    Check.fromGen gen (fun node ->
        let result = JsonNode.asJsonArray node
        result.Should().BeSuccess())

[<Fact>]
let ``asJsonValue with null returns a failure`` () =
    let result = JsonNode.asJsonValue null
    result.Should().BeFailure()

[<Fact>]
let ``asJsonValue with non JSON values returns a failure`` () =
    let gen = Gen.JsonNode.nonJsonValue

    Check.fromGen gen (fun node ->
        let result = JsonNode.asJsonValue node
        result.Should().BeFailure())

[<Fact>]
let ``asJsonValue with JSON values returns a success`` () =
    let gen = Gen.jsonValue

    Check.fromGen gen (fun node ->
        let result = JsonNode.asJsonValue node
        result.Should().BeSuccess())
