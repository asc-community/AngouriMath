module ReturnValues.Discrete

open Core
open MathFunctions.Discrete
open Xunit

let a = symbol "a"
let b = symbol "b"

[<Fact>]
let ``Conj test`` () = Assert.Equal(parse "a and b", conj a b)
[<Fact>]
let ``Disj test`` () = Assert.Equal(parse "a or b", disj a b)
[<Fact>]
let ``Impl test`` () = Assert.Equal(parse "a implies b", impl a b)
[<Fact>]
let ``Neg test`` () = Assert.Equal(parse "not a", neg a)
[<Fact>]
let ``Xor test`` () = Assert.Equal(parse "a xor b", xor a b)
[<Fact>]
let ``Equality test`` () = Assert.Equal(parse "a = b", equal a b)
[<Fact>]
let ``Greater test`` () = Assert.Equal(parse "a > b", greater a b)
[<Fact>]
let ``Less test`` () = Assert.Equal(parse "a < b", less a b)
[<Fact>]
let ``Greater or equal test`` () = Assert.Equal(parse "a >= b", greater_equal a b)
[<Fact>]
let ``Less or equal test`` () = Assert.Equal(parse "a <= b", less_equal a b)