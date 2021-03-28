module ReturnValues.CoreTest

open Xunit
open Core
open AngouriMath

[<Fact>]
let ``withSetting test`` () =
    let expr = parsed "a + b + c"
    let res = withSetting MathS.Diagnostic.OutputExplicit true (fun () -> expr.ToString())
    Assert.Equal("((a) + (b)) + (c)", res)
