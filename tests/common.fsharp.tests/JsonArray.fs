module JsonArrayTests

open Xunit
open common.fsharp
open Faqt
open FsCheck
open FsCheck.FSharp
open System.Text.Json.Nodes

let private generateNonEmptyJsonArray (gen: Gen<JsonArray>) =
    gen |> Gen.filter (fun jsonArray -> jsonArray.Count > 0)

[<Fact>]
let ``fromSeq has expected count`` () =
    let gen = Gen.listOf Gen.jsonNode

    Check.fromGen gen (fun nodes ->
        let jsonArray = JsonArray.fromSeq nodes
        jsonArray.Should().HaveLength(nodes.Length))

[<Fact>]
let ``getJsonObjects with non JSON objects returns a failure`` () =
    let gen =
        Gen.JsonNode.nonJsonObject |> Gen.generateJsonArray |> generateNonEmptyJsonArray

    Check.fromGen gen (fun jsonArray ->
        let result = JsonArray.getJsonObjects jsonArray
        result.Should().BeFailure())

[<Fact>]
let ``getJsonObjects with an empty array returns a success`` () =
    let jsonArray = JsonArray()
    let result = JsonArray.getJsonObjects jsonArray
    result.Should().BeSuccess().That.Should().BeEmpty()

[<Fact>]
let ``getJsonObjects with JSON objects returns a success`` () =
    let gen =
        Gen.JsonNode.jsonObject |> Gen.generateJsonArray |> generateNonEmptyJsonArray

    Check.fromGen gen (fun jsonArray ->
        let result = JsonArray.getJsonObjects jsonArray
        result.Should().BeSuccess().That.Should().HaveLength(jsonArray.Count))

[<Fact>]
let ``getJsonArrays with non JSON arrays returns a failure`` () =
    let gen =
        Gen.JsonNode.nonJsonArray |> Gen.generateJsonArray |> generateNonEmptyJsonArray

    Check.fromGen gen (fun jsonArray ->
        let result = JsonArray.getJsonArrays jsonArray
        result.Should().BeFailure())

[<Fact>]
let ``getJsonArrays with an empty array returns a success`` () =
    let jsonArray = JsonArray()
    let result = JsonArray.getJsonArrays jsonArray
    result.Should().BeSuccess().That.Should().BeEmpty()

[<Fact>]
let ``getJsonArrays with JSON arrays returns a success`` () =
    let gen =
        Gen.JsonNode.jsonArray |> Gen.generateJsonArray |> generateNonEmptyJsonArray

    Check.fromGen gen (fun jsonArray ->
        let result = JsonArray.getJsonArrays jsonArray
        result.Should().BeSuccess().That.Should().HaveLength(jsonArray.Count))

[<Fact>]
let ``getJsonValues with non JSON values returns a failure`` () =
    let gen =
        Gen.JsonNode.nonJsonValue |> Gen.generateJsonArray |> generateNonEmptyJsonArray

    Check.fromGen gen (fun jsonArray ->
        let result = JsonArray.getJsonValues jsonArray
        result.Should().BeFailure())

[<Fact>]
let ``getJsonValues with an empty array returns a success`` () =
    let jsonArray = JsonArray()
    let result = JsonArray.getJsonValues jsonArray
    result.Should().BeSuccess().That.Should().BeEmpty()

[<Fact>]
let ``getJsonValues with JSON values returns a success`` () =
    let gen =
        Gen.JsonNode.jsonValue |> Gen.generateJsonArray |> generateNonEmptyJsonArray

    Check.fromGen gen (fun jsonArray ->
        let result = JsonArray.getJsonValues jsonArray
        result.Should().BeSuccess().That.Should().HaveLength(jsonArray.Count))
