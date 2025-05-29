namespace common

open System
open System.Text.Json

type JsonError = private JsonError of Set<string>

[<RequireQualifiedAccess>]
module JsonError =
    let fromString message = Set.singleton message |> JsonError

    let internal fromException (exn: Exception) =
        match exn with
        | :? AggregateException as aggregateException ->
            aggregateException.Flatten().InnerExceptions
            |> Seq.map _.Message
            |> Seq.append [ aggregateException.Message ]
            |> Set.ofSeq
            |> JsonError
        | _ -> fromString exn.Message

    let getMessage (JsonError error) = Seq.head error

    let toJsonException (JsonError error) =
        match List.ofSeq error with
        | [ message ] -> new JsonException(message)
        | messages ->
            let aggregateException =
                messages
                |> Seq.map (fun message -> new JsonException(message) :> Exception)
                |> AggregateException

            new JsonException("Multiple errors, see inner exception for details..", aggregateException)

    let addError (JsonError newMessages) (JsonError existingMessages) =
        Set.union newMessages existingMessages |> JsonError

type JsonError with
    // Semigroup
    static member (+)(x: JsonError, y: JsonError) = x |> JsonError.addError y

type JsonResult<'a> =
    | Success of 'a
    | Failure of JsonError

[<RequireQualifiedAccess>]
module JsonResult =
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

    let throwIfFail jsonResult =
        jsonResult |> defaultWith (JsonError.toJsonException >> raise)

    let fromResult result =
        match result with
        | Ok x -> Success x
        | Error e -> Failure e

    let toResult jsonResult =
        match jsonResult with
        | Success x -> Ok x
        | Failure e -> Error e

type JsonResult<'a> with
    // Functor
    static member Map(x, f) =
        match x with
        | Success x -> JsonResult.succeed (f x)
        | Failure e -> JsonResult.fail e

    static member Unzip x =
        match x with
        | Success(x, y) -> JsonResult.succeed x, JsonResult.succeed y
        | Failure e -> JsonResult.fail e, JsonResult.fail e

    // Applicative
    static member Return x = JsonResult.succeed x

    static member (<*>)(f, x) = JsonResult.apply f x

    static member Lift2(f, x1, x2) =
        match x1, x2 with
        | Success x1, Success x2 -> f x1 x2 |> JsonResult.succeed
        | Failure e, Success _ -> JsonResult.fail e
        | Success _, Failure e -> JsonResult.fail e
        | Failure e1, Failure e2 -> JsonResult.fail (JsonError.addError e2 e1)

    static member Lift3(f, x1, x2, x3) =
        match x1, x2, x3 with
        | Success x1, Success x2, Success x3 -> f x1 x2 x3 |> JsonResult.succeed
        | Failure e1, Success _, Success _ -> JsonResult.fail e1
        | Failure e1, Success _, Failure e3 -> JsonResult.fail (JsonError.addError e3 e1)
        | Failure e1, Failure e2, Success _ -> JsonResult.fail (JsonError.addError e2 e1)
        | Failure e1, Failure e2, Failure e3 -> JsonResult.fail (e1 |> JsonError.addError e2 |> JsonError.addError e3)
        | Success _, Failure e2, Success _ -> JsonResult.fail e2
        | Success _, Failure e2, Failure e3 -> JsonResult.fail (JsonError.addError e2 e3)
        | Success _, Success _, Failure e3 -> JsonResult.fail e3

    // Zip applicative
    static member Pure x = JsonResult.succeed x

    static member (<.>)(f, x) = JsonResult.apply f x

    static member Zip(x1, x2) =
        match (x1, x2) with
        | Success x1, Success x2 -> JsonResult.succeed (x1, x2)
        | Failure e1, Success _ -> JsonResult.fail e1
        | Success _, Failure e2 -> JsonResult.fail e2
        | Failure e1, Failure e2 -> JsonResult.fail (JsonError.addError e2 e1)

    static member Map2(f, x1, x2) =
        match x1, x2 with
        | Success x1, Success x2 -> f x1 x2 |> JsonResult.succeed
        | Failure e, Success _ -> JsonResult.fail e
        | Success _, Failure e -> JsonResult.fail e
        | Failure e1, Failure e2 -> JsonResult.fail (JsonError.addError e2 e1)

    static member Map3(f, x1, x2, x3) =
        match x1, x2, x3 with
        | Success x1, Success x2, Success x3 -> f x1 x2 x3 |> JsonResult.succeed
        | Failure e1, Success _, Success _ -> JsonResult.fail e1
        | Failure e1, Success _, Failure e3 -> JsonResult.fail (JsonError.addError e3 e1)
        | Failure e1, Failure e2, Success _ -> JsonResult.fail (JsonError.addError e2 e1)
        | Failure e1, Failure e2, Failure e3 -> JsonResult.fail (e1 |> JsonError.addError e2 |> JsonError.addError e3)
        | Success _, Failure e2, Success _ -> JsonResult.fail e2
        | Success _, Failure e2, Failure e3 -> JsonResult.fail (JsonError.addError e2 e3)
        | Success _, Success _, Failure e3 -> JsonResult.fail e3

    // Monad
    static member Bind(x, f) =
        match x with
        | Success x -> f x
        | Failure e -> JsonResult.fail e

    static member (>>=)(x, f) = JsonResult<'a>.Bind(x, f)

    static member Join x =
        match x with
        | Success(Success x) -> JsonResult.succeed x
        | Success(Failure e) -> JsonResult.fail e
        | Failure e -> JsonResult.fail e

    // Foldable
    static member ToSeq x =
        match x with
        | Success x -> Seq.singleton x
        | Failure _ -> Seq.empty