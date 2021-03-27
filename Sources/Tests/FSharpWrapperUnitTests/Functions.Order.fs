module ReturnValues.Order

open Core
open Functions
open Xunit

let x = symbol "x"
let y = symbol "y"
let z = symbol "z"

[<Fact>]
let ``Derivative test`` () = Assert.Equal(parse "derivative(x, y)", derivative y x)
[<Fact>]
let ``Integral test`` () = Assert.Equal(parse "integral(x, y)", integral y x)
[<Fact>]
let ``Limit test`` () = Assert.Equal(parse "limit(x, y, z)", limited y z x)
[<Fact>]
let ``Differentiate test`` () = Assert.Equal(parse "2x + 2", differentiate x "x2 + 2x")
[<Fact>]
let ``Integrate test`` () = Assert.Equal(parse "sin(x) + x", integrate x "1 + cos(x)")
[<Fact>]
let ``Limited test`` () = Assert.Equal(parse "a", limit x 0 "a x / sin(x)")
