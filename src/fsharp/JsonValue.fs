﻿[<RequireQualifiedAccess>]
module common.JsonValue

open System
open System.Text.Json.Nodes
open System.Text.Json
open FSharpPlus

let private getString (jsonValue: JsonValue) = jsonValue.GetValue<obj>() |> string

let asString (jsonValue: JsonValue) =
    match jsonValue.GetValueKind() with
    | JsonValueKind.String -> getString jsonValue |> JsonResult.succeed
    | _ -> JsonResult.failWithMessage "JSON value is not a string"

let asInt (jsonValue: JsonValue) =
    match jsonValue.GetValueKind() with
    | JsonValueKind.Number ->
        match getString jsonValue |> Int32.TryParse with
        | true, x -> JsonResult.succeed x
        | _ -> JsonResult.failWithMessage "JSON value is not an integer"
    | _ -> JsonResult.failWithMessage "JSON value is not a number"

let asAbsoluteUri jsonValue =
    let errorMessage = "JSON value is not an absolute URI."

    asString jsonValue
    |> bind (fun stringValue ->
        match Uri.TryCreate(stringValue, UriKind.Absolute) with
        | true, uri when
            (match uri with
             | Null -> false
             | NonNull nonNullUri -> nonNullUri.HostNameType <> UriHostNameType.Unknown)
            ->
            JsonResult.succeed uri
        | _ -> JsonResult.failWithMessage errorMessage)
    |> JsonResult.setErrorMessage errorMessage

let asGuid jsonValue =
    let errorMessage = "JSON value is not a GUID."

    asString jsonValue
    |> bind (fun stringValue ->
        match Guid.TryParse(stringValue) with
        | true, guid -> JsonResult.succeed guid
        | _ -> JsonResult.failWithMessage errorMessage)

let asBool (jsonValue: JsonValue) =
    match jsonValue.GetValueKind() with
    | JsonValueKind.True -> JsonResult.succeed true
    | JsonValueKind.False -> JsonResult.succeed false
    | _ -> JsonResult.failWithMessage "JSON value is not a boolean."

let asDateTimeOffset (jsonValue: JsonValue) =
    let errorMessage = "JSON value is not a date time offset."

    match asString jsonValue with
    | JsonResult.Success stringValue ->
        match DateTimeOffset.TryParse(stringValue) with
        | true, dateTimeOffset -> JsonResult.succeed dateTimeOffset
        | _ -> JsonResult.failWithMessage errorMessage
    | JsonResult.Failure _ -> JsonResult.failWithMessage errorMessage
