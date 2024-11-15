[<RequireQualifiedAccess>]
module common.fsharp.JsonNode

open System.Text.Json.Nodes

let asJsonObject (node: JsonNode | null) =
    match node with
    | :? JsonObject as jsonObject -> JsonResult.succeed jsonObject
    | Null -> JsonResult.failWithMessage "JSON node is null"
    | _ -> JsonResult.failWithMessage "JSON node is not a JSON object"

let asJsonArray (node: JsonNode | null) =
    match node with
    | :? JsonArray as jsonArray -> JsonResult.succeed jsonArray
    | Null -> JsonResult.failWithMessage "JSON node is null"
    | _ -> JsonResult.failWithMessage "JSON node is not a JSON array"

let asJsonValue (node: JsonNode | null) =
    match node with
    | :? JsonValue as jsonValue -> JsonResult.succeed jsonValue
    | Null -> JsonResult.failWithMessage "JSON node is null"
    | _ -> JsonResult.failWithMessage "JSON node is not a JSON value"
