module AngouriMath.FSharp.Tests.Discrete

open AngouriMath.FSharp.Core
open AngouriMath.FSharp.Functions
open Xunit

let a = symbol "a"
let b = symbol "b"

[<Fact>]
let ``Conj test`` () = Assert.Equal(parsed "a and b", conjunction a b)
[<Fact>]
let ``Disj test`` () = Assert.Equal(parsed "a or b", disjunction a b)
[<Fact>]
let ``Impl test`` () = Assert.Equal(parsed "a implies b", implication a b)
[<Fact>]
let ``Neg test`` () = Assert.Equal(parsed "not a", negation a)
[<Fact>]
let ``Xor test`` () = Assert.Equal(parsed "a xor b", exDisjunction a b)
[<Fact>]
let ``Equality test`` () = Assert.Equal(parsed "a = b", equality a b)
[<Fact>]
let ``Greater test`` () = Assert.Equal(parsed "a > b", greater a b)
[<Fact>]
let ``Less test`` () = Assert.Equal(parsed "a < b", less a b)
[<Fact>]
let ``Greater or equal test`` () = Assert.Equal(parsed "a >= b", greaterOrEqual a b)
[<Fact>]
let ``Less or equal test`` () = Assert.Equal(parsed "a <= b", lessOrEqual a b)