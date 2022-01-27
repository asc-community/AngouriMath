module AngouriMath.Interactive.Tests.AggressiveOperators

open Xunit
open AngouriMath.FSharp.Functions
open AngouriMath.FSharp.Core
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

[<Fact>]
let ``equals test`` () = Assert.Equal(simplified (parsed "1 = 2"), (1 = 2))

[<Fact>]
let ``greaterThan test`` () = Assert.Equal(simplified (parsed "1 > 2"), (1 > 2))

[<Fact>]
let ``lessThan test`` () = Assert.Equal(simplified (parsed "1 < 2"), (1 < 2))

[<Fact>]
let ``greaterOrEqual test`` () = Assert.Equal(simplified (parsed "1 >= 2"), (1 >= 2))

[<Fact>]
let ``lessOrEqual test`` () = Assert.Equal(simplified (parsed "1 <= 2"), (1 <= 2))