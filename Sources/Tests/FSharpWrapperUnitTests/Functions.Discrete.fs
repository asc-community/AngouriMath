module ReturnValues.Discrete

open Core
open Functions
open Xunit

let a = symbol "a"
let b = symbol "b"

[<Fact>]
let ``Conj test`` () = Assert.Equal(parse "a and b", conjunction a b)
[<Fact>]
let ``Disj test`` () = Assert.Equal(parse "a or b", disjunction a b)
[<Fact>]
let ``Impl test`` () = Assert.Equal(parse "a implies b", implication a b)
[<Fact>]
let ``Neg test`` () = Assert.Equal(parse "not a", negation a)
[<Fact>]
let ``Xor test`` () = Assert.Equal(parse "a xor b", exDisjunction a b)
[<Fact>]
let ``Equality test`` () = Assert.Equal(parse "a = b", equal a b)
[<Fact>]
let ``Greater test`` () = Assert.Equal(parse "a > b", greater a b)
[<Fact>]
let ``Less test`` () = Assert.Equal(parse "a < b", less a b)
[<Fact>]
let ``Greater or equal test`` () = Assert.Equal(parse "a >= b", greaterOrEqual a b)
[<Fact>]
let ``Less or equal test`` () = Assert.Equal(parse "a <= b", lessOrEqual a b)