[<AutoOpen>]
module Assertions

open System.Runtime.CompilerServices
open Faqt
open Faqt.AssertionHelpers
open common

[<Extension>]
type JsonErrorAssertions =
    static member HaveMessage(t: Testable<JsonError>, expected, ?because) : AndDerived<JsonError, string> =
        use _ = t.Assert()

        let errorMessage = JsonError.getMessage t.Subject

        if errorMessage <> expected then
            t.With("Expected", expected).With("But was", errorMessage).Fail(because)

        AndDerived(t, errorMessage)

[<Extension>]
type JsonResultAssertions =
    [<Extension>]
    static member BeSuccess<'a>(t: Testable<JsonResult<'a>>, ?because) : AndDerived<JsonResult<'a>, 'a> =
        use _ = t.Assert()

        t.Subject
        |> JsonResult.map (fun a -> AndDerived(t, a))
        |> JsonResult.defaultWith (fun error ->
            t.With("But was", "Failure").With("Error message", JsonError.getMessage error).Fail(because))

    [<Extension>]
    static member BeFailure<'a>(t: Testable<JsonResult<'a>>, ?because) : AndDerived<JsonResult<'a>, JsonError> =
        use _ = t.Assert()

        t.Subject
        |> JsonResult.map (fun a -> t.With("But was", "Success").With("Value", a).Fail(because))
        |> JsonResult.defaultWith (fun error -> AndDerived(t, error))

[<Extension>]
type SeqAssertions =
    [<Extension>]
    static member HaveSameCountAs<'a, 'b>(t: Testable<seq<'a>>, other: seq<'b>, ?because) : And<seq<'a>> =
        use _ = t.Assert()

        let actualCount = Seq.length t.Subject
        let expectedCount = Seq.length other

        if actualCount <> expectedCount then
            t.With("Expected", expectedCount).With("But was", actualCount).Fail(because)
        else
            And(t)
