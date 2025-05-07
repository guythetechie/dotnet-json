[<RequireQualifiedAccess>]
module common.JsonArray

open FSharpPlus
open System.Text.Json.Nodes

let private toSeq (jsonArray: JsonArray) = jsonArray |> seq

let fromSeq seq = Array.ofSeq seq |> JsonArray

let private getResultSeq toJsonResult toErrorMessage (jsonArray: JsonArray) =
    jsonArray
    |> toSeq
    |> traversei (fun index node ->
        let replaceErrorMessage = toErrorMessage index |> JsonResult.setErrorMessage
        toJsonResult node |> replaceErrorMessage)

let asJsonObjects jsonArray =
    getResultSeq JsonNode.asJsonObject (fun index -> $"Element at index {index} is not a JSON object.") jsonArray

let asJsonArrays jsonArray =
    getResultSeq JsonNode.asJsonArray (fun index -> $"Element at index {index} is not a JSON array.") jsonArray

let asJsonValues jsonArray =
    getResultSeq JsonNode.asJsonValue (fun index -> $"Element at index {index} is not a JSON value.") jsonArray

let getJsonObjects jsonArray =
    jsonArray
    |> toSeq
    |> Seq.choose (fun node ->
        match node with
        | :? JsonObject as jsonObject -> Some jsonObject
        | _ -> None)

let getJsonArrays jsonArray =
    jsonArray
    |> toSeq
    |> Seq.choose (fun node ->
        match node with
        | :? JsonArray as jsonArray -> Some jsonArray
        | _ -> None)

let getJsonValues jsonArray =
    jsonArray
    |> toSeq
    |> Seq.choose (fun node ->
        match node with
        | :? JsonValue as jsonValue -> Some jsonValue
        | _ -> None)
