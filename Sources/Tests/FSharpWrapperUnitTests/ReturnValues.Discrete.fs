module ReturnValues.Discrete

open Core
open MathFunctions.Discrete
open Xunit

let a = symbol "a"
let b = symbol "b"

[<Fact>]
let ``Conj test`` () = Assert.Equal(parseSilent "a and b", conj a b)
[<Fact>]
let ``Disj test`` () = Assert.Equal(parseSilent "a or b", disj a b)
[<Fact>]
let ``Impl test`` () = Assert.Equal(parseSilent "a implies b", impl a b)
[<Fact>]
let ``Neg test`` () = Assert.Equal(parseSilent "not a", neg a)
[<Fact>]
let ``Xor test`` () = Assert.Equal(parseSilent "a xor b", xor a b)
[<Fact>]
let ``Equality test`` () = Assert.Equal(parseSilent "a = b", equal a b)
[<Fact>]
let ``Greater test`` () = Assert.Equal(parseSilent "a > b", greater a b)
[<Fact>]
let ``Less test`` () = Assert.Equal(parseSilent "a < b", less a b)
[<Fact>]
let ``Greater or equal test`` () = Assert.Equal(parseSilent "a >= b", greater_equal a b)
[<Fact>]
let ``Less or equal test`` () = Assert.Equal(parseSilent "a <= b", less_equal a b)