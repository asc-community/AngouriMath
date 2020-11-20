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