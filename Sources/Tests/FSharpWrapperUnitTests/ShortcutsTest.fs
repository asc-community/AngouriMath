module ShortcutsTest

open Xunit
open Shortcuts
open Core

[<Fact>]
let ``Test dy/dx`` () =
    Assert.Equal(parse "a + cos(x)", ``dy/dx`` "a x + sin(x)")
[<Fact>]
let ``Test int [dx]`` () =
    Assert.Equal(parse "sin(x) + a x", ``int [dx]`` "a + cos(x)")
[<Fact>]
let ``Test lim x->+oo`` () =
    Assert.Equal(parse "6", ``lim x->+oo`` "(6x6 + 3x3 + a x) / (x6 - 4x)")
[<Fact>]
let ``Test lim x->-oo`` () =
    Assert.Equal(parse "-oo", ``lim x->-oo`` "2x")
[<Fact>]
let ``Test lim x->0`` () =
    Assert.Equal((parse "-1/6").InnerSimplified, ``lim x->0`` "(sin(x) - x) / x3")
[<Fact>]
let ``Test -|>``() =
    Assert.Equal(parse "25", ("x", 5) -|> (parse "5x"))
[<Fact>]
let ``Test <|-``() =
    Assert.Equal(parse "25",  (parse "5x") <|- ("x", 5))
[<Fact>]
let ``Test -|><|-``() =
    Assert.Equal(parse "125",  ("y", 100) -|> (parse "5x + y") <|- ("x", 5))