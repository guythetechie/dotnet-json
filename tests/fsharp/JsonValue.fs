module JsonValueTests

open Xunit
open common
open Faqt


[<Fact>]
let ``asString with a non string value fails`` () =
    let gen = Gen.JsonValue.nonString

    Check.fromGen gen (fun value ->
        let result = JsonValue.asString value
        result.Should().BeFailure())

[<Fact>]
let ``asString with a string value succeeds`` () =
    let gen = Gen.JsonValue.string

    Check.fromGen gen (fun value ->
        let result = JsonValue.asString value
        result.Should().BeSuccess())

[<Fact>]
let ``asInt with a non integer value fails`` () =
    let gen = Gen.JsonValue.nonInteger

    Check.fromGen gen (fun value ->
        let result = JsonValue.asInt value
        result.Should().BeFailure())

[<Fact>]
let ``asInt with an integer value succeeds`` () =
    let gen = Gen.JsonValue.integer

    Check.fromGen gen (fun value ->
        let result = JsonValue.asInt value
        result.Should().BeSuccess())

[<Fact>]
let ``asAbsoluteUri with a non URI value fails`` () =
    let gen = Gen.JsonValue.nonAbsoluteUri

    Check.fromGen gen (fun value ->
        let result = JsonValue.asAbsoluteUri value
        result.Should().BeFailure())

[<Fact>]
let ``asAbsoluteUri with a URI value succeeds`` () =
    let gen = Gen.JsonValue.absoluteUri

    Check.fromGen gen (fun value ->
        let result = JsonValue.asAbsoluteUri value
        result.Should().BeSuccess())

[<Fact>]
let ``asGuid with a non GUID value fails`` () =
    let gen = Gen.JsonValue.nonGuid

    Check.fromGen gen (fun value ->
        let result = JsonValue.asGuid value
        result.Should().BeFailure())

[<Fact>]
let ``asGuid with a GUID value succeeds`` () =
    let gen = Gen.JsonValue.guid

    Check.fromGen gen (fun value ->
        let result = JsonValue.asGuid value
        result.Should().BeSuccess())

[<Fact>]
let ``asBool with a non boolean value fails`` () =
    let gen = Gen.JsonValue.nonBool

    Check.fromGen gen (fun value ->
        let result = JsonValue.asBool value
        result.Should().BeFailure())

[<Fact>]
let ``asBool with a boolean value succeeds`` () =
    let gen = Gen.JsonValue.bool

    Check.fromGen gen (fun value ->
        let result = JsonValue.asBool value
        result.Should().BeSuccess())
