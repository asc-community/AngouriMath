module ReturnValues.Order

open Core
open MathFunctions.Order
open Xunit

let x = symbol "x"
let y = symbol "y"
let z = symbol "z"

[<Fact>]
let ``Derivative test`` () = Assert.Equal(parseSilent "derivative(x, y)", derivative y x)
[<Fact>]
let ``Integral test`` () = Assert.Equal(parseSilent "integral(x, y)", integral y x)
[<Fact>]
let ``Limit test`` () = Assert.Equal(parseSilent "limit(x, y, z)", limited y z x)
