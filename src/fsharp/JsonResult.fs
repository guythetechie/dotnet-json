namespace common.fsharp

open System
open System.Text.Json

type JsonError = private JsonError of JsonException

[<RequireQualifiedAccess>]
module JsonError =
    let fromString message = JsonException(message) |> JsonError

    let internal fromException jsonException = JsonError jsonException

    let getMessage (JsonError error) = error.Message

    let toJsonException (JsonError error) = error

    let addError newError existingError =
        let innerExceptions =
            let newJsonException = toJsonException newError
            let existingJsonException = toJsonException existingError

            match existingJsonException.InnerException with
            | :? AggregateException as aggregateException ->
                Seq.append aggregateException.InnerExceptions [ newJsonException ]
            | _ -> Seq.append [ existingJsonException ] [ newJsonException ]

        let jsonException =
            new JsonException(
                "Multiple errors, see inner exception for details..",
                new AggregateException(innerExceptions)
            )

        fromException jsonException

type JsonResult<'a> =
    private
    | Success of 'a
    | Failure of JsonError

[<RequireQualifiedAccess>]
module JsonResult =
    let isSuccess jsonResult =
        match jsonResult with
        | Success _ -> true
        | Failure _ -> false

    let isFailure jsonResult =
        match jsonResult with
        | Success _ -> false
        | Failure _ -> true

    let succeed x = Success x

    let fail e = Failure e

    let failWithMessage message = JsonError.fromString message |> fail

    let replaceErrorWith f jsonResult =
        match jsonResult with
        | Failure e -> f e |> fail
        | _ -> jsonResult

    let setErrorMessage message jsonResult =
        replaceErrorWith (fun _ -> JsonError.fromString message) jsonResult

    let map f jsonResult =
        match jsonResult with
        | Success x -> succeed (f x)
        | Failure e -> fail e

    let apply f x =
        match f, x with
        | Success f, Success x -> succeed (f x)
        | Failure e, Success _ -> fail e
        | Success _, Failure e -> fail e
        | Failure e1, Failure e2 -> fail (JsonError.addError e2 e1)

    let defaultWith f jsonResult =
        match jsonResult with
        | Success x -> x
        | Failure jsonError -> f jsonError

type JsonResult<'a> with

    static member Return x = JsonResult.succeed x

    static member (<*>)(f, x) = JsonResult.apply f x

    static member (<*>)(f, x) = JsonResult.map f x

    static member (<|>)(x, y) =
        match x, y with
        | Success _, _ -> x
        | _, Success _ -> y
        | Failure e1, Failure e2 -> x

    static member (>>=)(x, f) =
        match x with
        | Success a -> f a
        | Failure error -> JsonResult.fail error

    static member Map(x, f) = JsonResult.map f x

    static member Lift2(f, x1, x2) =
        match x1, x2 with
        | Success x1, Success x2 -> f x1 x2 |> JsonResult.succeed
        | Failure e, _ -> JsonResult.fail e
        | _, Failure e -> JsonResult.fail e

    static member Unzip x =
        match x with
        | Success(x, y) -> JsonResult.succeed x, JsonResult.succeed y
        | Failure e -> JsonResult.fail e, JsonResult.fail e
