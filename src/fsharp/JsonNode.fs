[<RequireQualifiedAccess>]
module common.JsonNode

open System
open System.Text.Json
open System.Text.Json.Nodes
open System.IO

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

let private ifNoneNullable option =
    option |> Option.map Nullable |> Option.defaultValue (Nullable())

let private ifNoneDefault option =
    option |> Option.defaultValue Unchecked.defaultof<'a>

let fromStreamWithOptions (stream: Stream | null) nodeOptions documentOptions =
    async {
        let! cancellationToken = Async.CancellationToken

        match stream with
        | Null -> return JsonResult.failWithMessage "Stream is null."
        | NonNull stream ->
            try
                let nodeOptions = ifNoneNullable nodeOptions
                let documentOptions = ifNoneDefault documentOptions

                match!
                    JsonNode.ParseAsync(stream, nodeOptions, documentOptions, cancellationToken = cancellationToken)
                    |> Async.AwaitTask
                with
                | Null -> return JsonResult.failWithMessage "Deserialization returned a null result."
                | NonNull node -> return JsonResult.succeed node
            with exn ->
                return JsonError.fromException exn |> JsonResult.fail
    }

let fromStream stream = fromStreamWithOptions stream None None

let fromBinaryDataWithOptions (data: BinaryData | null) nodeOptions =
    try
        match data with
        | Null -> JsonResult.failWithMessage "Binary data is null."
        | NonNull data ->
            let nodeOptions = ifNoneNullable nodeOptions

            match JsonNode.Parse(data, nodeOptions) with
            | Null -> JsonResult.failWithMessage "Deserialization returned a null result."
            | NonNull node -> JsonResult.succeed node
    with exn ->
        JsonError.fromException exn |> JsonResult.fail

let fromBinaryData data = fromBinaryDataWithOptions data None

let toBinaryData (node: JsonNode) = BinaryData.FromObjectAsJson(node)

let toStream (node: JsonNode) = toBinaryData node |> _.ToStream()
