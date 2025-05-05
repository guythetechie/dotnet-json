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
let ``asJsonObjects with non JSON objects returns a failure`` () =
    let gen =
        Gen.JsonNode.nonJsonObject |> Gen.generateJsonArray |> generateNonEmptyJsonArray

    Check.fromGen gen (fun jsonArray ->
        let result = JsonArray.asJsonObjects jsonArray
        result.Should().BeFailure())

[<Fact>]
let ``asJsonObjects with an empty array returns a success`` () =
    let jsonArray = JsonArray()
    let result = JsonArray.asJsonObjects jsonArray
    result.Should().BeSuccess().That.Should().BeEmpty()

[<Fact>]
let ``asJsonObjects with JSON objects returns a success`` () =
    let gen =
        Gen.JsonNode.jsonObject |> Gen.generateJsonArray |> generateNonEmptyJsonArray

    Check.fromGen gen (fun jsonArray ->
        let result = JsonArray.asJsonObjects jsonArray
        result.Should().BeSuccess().That.Should().HaveLength(jsonArray.Count))

[<Fact>]
let ``asJsonArrays with non JSON arrays returns a failure`` () =
    let gen =
        Gen.JsonNode.nonJsonArray |> Gen.generateJsonArray |> generateNonEmptyJsonArray

    Check.fromGen gen (fun jsonArray ->
        let result = JsonArray.asJsonArrays jsonArray
        result.Should().BeFailure())

[<Fact>]
let ``asJsonArrays with an empty array returns a success`` () =
    let jsonArray = JsonArray()
    let result = JsonArray.asJsonArrays jsonArray
    result.Should().BeSuccess().That.Should().BeEmpty()

[<Fact>]
let ``asJsonArrays with JSON arrays returns a success`` () =
    let gen =
        Gen.JsonNode.jsonArray |> Gen.generateJsonArray |> generateNonEmptyJsonArray

    Check.fromGen gen (fun jsonArray ->
        let result = JsonArray.asJsonArrays jsonArray
        result.Should().BeSuccess().That.Should().HaveLength(jsonArray.Count))

[<Fact>]
let ``asJsonValues with non JSON values returns a failure`` () =
    let gen =
        Gen.JsonNode.nonJsonValue |> Gen.generateJsonArray |> generateNonEmptyJsonArray

    Check.fromGen gen (fun jsonArray ->
        let result = JsonArray.asJsonValues jsonArray
        result.Should().BeFailure())

[<Fact>]
let ``asJsonValues with an empty array returns a success`` () =
    let jsonArray = JsonArray()
    let result = JsonArray.asJsonValues jsonArray
    result.Should().BeSuccess().That.Should().BeEmpty()

[<Fact>]
let ``asJsonValues with JSON values returns a success`` () =
    let gen =
        Gen.JsonNode.jsonValue |> Gen.generateJsonArray |> generateNonEmptyJsonArray

    Check.fromGen gen (fun jsonArray ->
        let result = JsonArray.asJsonValues jsonArray
        result.Should().BeSuccess().That.Should().HaveLength(jsonArray.Count))

[<Fact>]
let ``getJsonObjects returns only JSON objects`` () =
    let gen =
        gen {
            let! jsonObjects = Gen.jsonObject |> Gen.arrayOf

            let! jsonArray =
                let jsonObjectsAsNodes =
                    jsonObjects |> Array.map (fun jsonObject -> jsonObject :> JsonNode)

                Gen.JsonNode.nonJsonObject
                |> Gen.arrayOf
                |> Gen.map (Array.append jsonObjectsAsNodes)
                |> Gen.bind Gen.shuffle
                |> Gen.map JsonArray.fromSeq

            return jsonObjects, jsonArray
        }

    Check.fromGen gen (fun (jsonObjects, jsonArray) ->
        let result = JsonArray.getJsonObjects jsonArray
        result.Should().HaveSameCountAs(jsonObjects))

[<Fact>]
let ``getJsonArrays returns only JSON arrays`` () =
    let gen =
        gen {
            let! jsonArrays = Gen.jsonArray |> Gen.arrayOf

            let! jsonArray =
                let jsonArraysAsNodes =
                    jsonArrays |> Array.map (fun jsonArray -> jsonArray :> JsonNode)

                Gen.JsonNode.nonJsonArray
                |> Gen.arrayOf
                |> Gen.map (Array.append jsonArraysAsNodes)
                |> Gen.bind Gen.shuffle
                |> Gen.map JsonArray.fromSeq

            return jsonArrays, jsonArray
        }

    Check.fromGen gen (fun (jsonArrays, jsonArray) ->
        let result = JsonArray.getJsonArrays jsonArray
        result.Should().HaveSameCountAs(jsonArrays))

[<Fact>]
let ``getJsonValues returns only JSON values`` () =
    let gen =
        gen {
            let! jsonValues = Gen.jsonValue |> Gen.arrayOf

            let! jsonArray =
                let jsonValuesAsNodes =
                    jsonValues |> Array.map (fun jsonValue -> jsonValue :> JsonNode)

                Gen.JsonNode.nonJsonValue
                |> Gen.arrayOf
                |> Gen.map (Array.append jsonValuesAsNodes)
                |> Gen.bind Gen.shuffle
                |> Gen.map JsonArray.fromSeq

            return jsonValues, jsonArray
        }

    Check.fromGen gen (fun (jsonValues, jsonArray) ->
        let result = JsonArray.getJsonValues jsonArray
        result.Should().HaveSameCountAs(jsonValues))
