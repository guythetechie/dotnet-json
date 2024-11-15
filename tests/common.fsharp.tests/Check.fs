[<RequireQualifiedAccess>]
module Check

open FsCheck
open FsCheck.FSharp

let fromGenWithConfig config gen f =
    let arb = Arb.fromGen gen
    let property = Prop.forAll arb (f >> ignore)
    Check.One(config, property)

let runSpecificTest replay gen f =
    let (seed, gamma) = replay
    let config = Config.VerboseThrowOnFailure.WithMaxTest(1).WithReplay(seed, gamma)
    fromGenWithConfig config gen f

let fromGen gen f =
    let config = Config.QuickThrowOnFailure.WithMaxTest(1000)
    fromGenWithConfig config gen f
