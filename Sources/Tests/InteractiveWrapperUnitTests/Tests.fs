module Tests

open Xunit
open Microsoft.DotNet.Interactive.Formatting
open AngouriMath.FSharp.Core

[<Fact>]
let ``Test output for Entity`` () =
    let entity = parsed "x / 2"
    let html = entity.ToDisplayString("text/html")
    Assert.Equal(html, "df")