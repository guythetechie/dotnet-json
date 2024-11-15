[<RequireQualifiedAccess>]
module Gen

open FsCheck
open FsCheck.FSharp
open System
open System.Text.Json.Nodes
open System.Collections.Generic
open System.Text.Json

let generateDefault<'a> () = ArbMap.defaults |> ArbMap.generate<'a>

let jsonValue =
    Gen.oneof
        [ generateDefault<int> () |> Gen.map JsonValue.Create
          generateDefault<string> () |> Gen.map JsonValue.Create
          generateDefault<bool> () |> Gen.map JsonValue.Create
          generateDefault<double> ()
          |> Gen.filter (Double.IsInfinity >> not)
          |> Gen.filter (Double.IsNaN >> not)
          |> Gen.map JsonValue.Create
          generateDefault<byte> () |> Gen.map JsonValue.Create
          generateDefault<Guid> () |> Gen.map JsonValue.Create ]

let toJsonNode gen =
    gen |> Gen.map (fun value -> value :> JsonNode)

let private jsonValueAsNode = toJsonNode jsonValue

let generateJsonArray (nodeGen: Gen<JsonNode | null>) =
    Gen.arrayOf nodeGen |> Gen.map JsonArray

let generateJsonObject (nodeGen: Gen<JsonNode | null>) =
    Gen.zip (generateDefault<string> ()) nodeGen
    |> Gen.listOf
    |> Gen.map (Seq.distinctBy (fun (first, second) -> first.ToUpperInvariant()))
    |> Gen.map (Seq.map KeyValuePair.Create)
    |> Gen.map JsonObject

let jsonNode =
    let rec generateJsonNode size =
        if size < 1 then
            jsonValueAsNode
        else
            let reducedSizeGen = generateJsonNode (size / 2)

            Gen.oneof
                [ jsonValueAsNode
                  generateJsonArray reducedSizeGen |> toJsonNode
                  generateJsonObject reducedSizeGen |> toJsonNode ]

    Gen.sized generateJsonNode

let jsonObject = generateJsonObject jsonNode

let jsonArray = generateJsonArray jsonNode

[<RequireQualifiedAccess>]
module JsonValue =
    let string = generateDefault<string>() |> Gen.map JsonValue.Create

    let nonString =
        jsonValue |> Gen.filter (fun value -> value.GetValueKind() <> JsonValueKind.String)

    let integer = generateDefault<int>() |> Gen.map JsonValue.Create

    let nonInteger =
        jsonValue
        |> Gen.filter (fun value ->
            match value.GetValue<obj>() with
            | :? int -> false
            | :? byte -> false
            | _ -> true)

    let absoluteUri =
        generateDefault<NonWhiteSpaceString> ()
        |> Gen.map (fun value -> $"https://{value.Get}")
        |> Gen.filter (fun uri -> Uri.TryCreate(uri, UriKind.Absolute) |> fst)
        |> Gen.map JsonValue.Create

    let nonAbsoluteUri =
        generateDefault<obj>()
        |> Gen.filter (fun value ->
            match Uri.TryCreate(value.ToString(), UriKind.Absolute) with
            | true, _ -> false
            | _ -> true)
        |> Gen.map JsonValue.Create

    let guid =
        generateDefault<Guid> ()
        |> Gen.map (fun value -> value.ToString())
        |> Gen.map JsonValue.Create

    let nonGuid =
        generateDefault<obj>()
        |> Gen.filter (fun value ->
            match Guid.TryParse(value.ToString()) with
            | true, _ -> false
            | _ -> true)
        |> Gen.map JsonValue.Create

    let bool = Gen.elements [true; false]  |> Gen.map JsonValue.Create

    let nonBool =
        generateDefault<obj> ()
        |> Gen.filter (fun value ->
            match value with
            | :? bool -> false
            | _ -> true)
        |> Gen.map JsonValue.Create

[<RequireQualifiedAccess>]
module JsonNode =
    let jsonObject = toJsonNode jsonObject

    let nonJsonObject = Gen.oneof [toJsonNode jsonValue; toJsonNode jsonArray]

    let jsonArray = toJsonNode jsonArray

    let nonJsonArray = Gen.oneof [toJsonNode jsonValue; toJsonNode jsonObject]

    let jsonValue = jsonValueAsNode

    let nonJsonValue = Gen.oneof [toJsonNode jsonArray; toJsonNode jsonObject]