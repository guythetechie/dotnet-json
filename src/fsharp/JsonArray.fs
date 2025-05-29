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

let getJsonObjects jsonArray =
    getResultSeq JsonNode.asJsonObject (fun index -> $"Element at index {index} is not a JSON object.") jsonArray

let getJsonArrays jsonArray =
    getResultSeq JsonNode.asJsonArray (fun index -> $"Element at index {index} is not a JSON array.") jsonArray

let getJsonValues jsonArray =
    getResultSeq JsonNode.asJsonValue (fun index -> $"Element at index {index} is not a JSON value.") jsonArray
