module AngouriMath.FSharp.Tests.Core

open Xunit
open AngouriMath.FSharp.Core
open AngouriMath

[<Fact>]
let ``withSetting test`` () =
    let expr = parsed "a + b + c"
    let res = withSetting MathS.Diagnostic.OutputExplicit true (fun () -> expr.ToString())
    Assert.Equal("((a) + (b)) + (c)", res)

[<Fact>]
let ``withSetting test with exception`` () =
    let expr = parsed "a + b + c"
    let expectedValue = MathS.Settings.MaxExpansionTermCount.Value
    try
        try 
            let res = withSetting MathS.Settings.MaxExpansionTermCount 100L (fun() ->
                let a = "x + 2"
                parsed "x + "
            )
            ()
        with
            | _ -> ()
    finally
        let actualValue = MathS.Settings.MaxExpansionTermCount.Value
        Assert.Equal(expectedValue, actualValue)