module Tests.AggressiveOperators

open Xunit
open Functions
open Core
open AngouriMath.Interactive.AggressiveOperators

[<Fact>]
let ``Int / int is rational`` () =
    let threeOverTwo = 3 / 2
    Assert.IsAssignableFrom<AngouriMath.Entity.Number.Rational>(threeOverTwo) |> ignore
    Assert.Equal(simplified (parsed "3 / 2"), threeOverTwo)

[<Fact>]
let ``Int + real is real`` () =
    let num = 3 + 4.5
    Assert.IsAssignableFrom<AngouriMath.Entity.Number.Rational>(num) |> ignore
    Assert.Equal(simplified (parsed "3 + 4.5"), num)

[<Fact>]
let ``Int - real is real`` () =
    let num = 3 - 4.5
    Assert.IsAssignableFrom<AngouriMath.Entity.Number.Rational>(num) |> ignore
    Assert.Equal(simplified (parsed "3 - 4.5"), num)

[<Fact>]
let ``Int * real is real`` () =
    let num = 3 * 4.5
    Assert.IsAssignableFrom<AngouriMath.Entity.Number.Rational>(num) |> ignore
    Assert.Equal(simplified (parsed "3 * 4.5"), num)

[<Fact>]
let ``Int ** big number is real`` () =
    let num = 3 ** "1000"
    Assert.IsAssignableFrom<AngouriMath.Entity.Number.Integer>(num) |> ignore
    Assert.Equal(simplified (parsed "3 ^ 1000"), num)