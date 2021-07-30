module Tests

open System
open Xunit
open AngouriMath.Terminal.Lib.FSharpInteractive

let kernel =
    match createKernel () with
    | Result.Error error -> raise (Exception error)
    | Result.Ok k -> k

[<Fact>]
let ``Test 1`` () =
    execute kernel "1 + 1"
    |> (fun actual -> PlainTextSuccess "2", actual)
    |> Assert.Equal

[<Fact>]
let ``Test 2`` () =
    execute kernel "8 / 6"
    |> (fun actual -> PlainTextSuccess "4/3", actual)
    |> Assert.Equal

[<Fact>]
let ``Test 3`` () =
    execute kernel "solutions \"x\" \"x2 = 4\""
    |> (fun actual -> PlainTextSuccess "{ -2, 2 }", actual)
    |> Assert.Equal

[<Fact>]
let ``Test 4`` () =
    execute kernel "let x = 4"
    |> (fun actual -> VoidSuccess, actual)
    |> Assert.Equal

[<Fact>]
let ``Test 5`` () =
    execute kernel "let x ="
    |> (fun actual -> 
        match actual with
        | Error error -> Assert.Contains("invalid", error)
        | _ -> Assert.True(false, $"Expected an error. Got {actual} instead")
        )
