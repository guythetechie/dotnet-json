module JsonNodeTests

open System
open System.IO
open Xunit
open Faqt
open common

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

[<Fact>]
let ``fromStream with null returns a failure`` () =
    async {
        let! result = JsonNode.fromStream null
        result.Should().BeFailure() |> ignore
    }
    |> Async.RunSynchronously

[<Fact>]
let ``fromStream with an empty stream fails`` () =
    async {
        use stream = new MemoryStream()
        let! result = JsonNode.fromStream stream
        result.Should().BeFailure() |> ignore
    }
    |> Async.RunSynchronously

[<Fact>]
let ``fromStream with a valid stream succeeds`` () =
    let gen = Gen.jsonNode

    Check.fromGen gen (fun node ->
        async {
            use stream = BinaryData.FromObjectAsJson(node).ToStream()
            let! result = JsonNode.fromStream stream

            result
                .Should()
                .BeSuccess()
                .WhoseValue.GetValueKind()
                .Should()
                .Be(node.GetValueKind())
            |> ignore
        }
        |> Async.RunSynchronously)

[<Fact>]
let ``fromBinaryData with null returns a failure`` () =
    let result = JsonNode.fromBinaryData null
    result.Should().BeFailure() |> ignore

[<Fact>]
let ``fromBinaryData with valid data succeeds`` () =
    let gen = Gen.jsonNode

    Check.fromGen gen (fun node ->
        let data = BinaryData.FromObjectAsJson(node)
        let result = JsonNode.fromBinaryData data

        result
            .Should()
            .BeSuccess()
            .WhoseValue.GetValueKind()
            .Should()
            .Be(node.GetValueKind())
        |> ignore)
