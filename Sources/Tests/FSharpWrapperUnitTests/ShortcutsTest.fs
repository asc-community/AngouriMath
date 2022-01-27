module AngouriMath.FSharp.Tests.Shortcuts

open Xunit
open AngouriMath.FSharp.Shortcuts
open AngouriMath.FSharp.Core

[<Fact>]
let ``Test d/dx`` () =
    Assert.Equal(parsed "a + cos(x)", ``d/dx`` "a x + sin(x)")
[<Fact>]
let ``Test int [dx]`` () =
    Assert.Equal(parsed "sin(x) + a x", ``int [dx]`` "a + cos(x)")
[<Fact>]
let ``Test lim x->+oo`` () =
    Assert.Equal(parsed "6", ``lim x->+oo`` "(6x6 + 3x3 + a x) / (x6 - 4x)")
[<Fact>]
let ``Test lim x->-oo`` () =
    Assert.Equal(parsed "-oo", ``lim x->-oo`` "2x")
[<Fact>]
let ``Test lim x->0`` () =
    Assert.Equal((parsed "-1/6").InnerSimplified, ``lim x->0`` "(sin(x) - x) / x3")
[<Fact>]
let ``Test -|>``() =
    Assert.Equal(parsed "25", ("x", 5) -|> (parsed "5x"))
[<Fact>]
let ``Test <|-``() =
    Assert.Equal(parsed "25",  (parsed "5x") <|- ("x", 5))
[<Fact>]
let ``Test -|><|-``() =
    Assert.Equal(parsed "125",  ("y", 100) -|> (parsed "5x + y") <|- ("x", 5))