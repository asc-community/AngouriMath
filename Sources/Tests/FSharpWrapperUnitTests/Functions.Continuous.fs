module AngouriMath.FSharp.Tests.Continuous

open AngouriMath.FSharp.Core
open AngouriMath.FSharp.Functions
open Xunit

let x = symbol "x"

[<Fact>]
let ``Test symbol x`` () = Assert.Equal(parsed "x", symbol "x")
[<Fact>]
let ``Test symbol y`` () = Assert.Equal(parsed "y", symbol "y")

[<Fact>]
let ``Sin test`` () = Assert.Equal(parsed "sin(x)", sin x)
[<Fact>]
let ``Cos test`` () = Assert.Equal(parsed "cos(x)", cos x)
[<Fact>]
let ``Tan test`` () = Assert.Equal(parsed "tan(x)", tan x)
[<Fact>]
let ``Cot test`` () = Assert.Equal(parsed "cot(x)", cot x)
[<Fact>]
let ``Sec test`` () = Assert.Equal(parsed "sec(x)", sec x)
[<Fact>]
let ``Csc test`` () = Assert.Equal(parsed "csc(x)", csc x)
[<Fact>]
let ``Asin test`` () = Assert.Equal(parsed "asin(x)", asin x)
[<Fact>]
let ``Acos test`` () = Assert.Equal(parsed "acos(x)", acos x)
[<Fact>]
let ``Atan test`` () = Assert.Equal(parsed "atan(x)", atan x)
[<Fact>]
let ``Acot test`` () = Assert.Equal(parsed "acot(x)", acot x)
[<Fact>]
let ``Asec test`` () = Assert.Equal(parsed "asec(x)", asec x)
[<Fact>]
let ``Acsc test`` () = Assert.Equal(parsed "acsc(x)", acsc x)
[<Fact>]
let ``Sinh test`` () = Assert.Equal(parsed "sinh(x)", sinh x)
[<Fact>]
let ``Cosh test`` () = Assert.Equal(parsed "cosh(x)", cosh x)
[<Fact>]
let ``Tanh test`` () = Assert.Equal(parsed "tanh(x)", tanh x)
[<Fact>]
let ``Coth test`` () = Assert.Equal(parsed "coth(x)", coth x)
[<Fact>]
let ``Sech test`` () = Assert.Equal(parsed "sech(x)", sech x)
[<Fact>]
let ``Csch test`` () = Assert.Equal(parsed "csch(x)", csch x)
[<Fact>]
let ``Asinh test`` () = Assert.Equal(parsed "asinh(x)", asinh x)
[<Fact>]
let ``Acosh test`` () = Assert.Equal(parsed "acosh(x)", acosh x)
[<Fact>]
let ``Atanh test`` () = Assert.Equal(parsed "atanh(x)", atanh x)
[<Fact>]
let ``Acoth test`` () = Assert.Equal(parsed "acoth(x)", acoth x)
[<Fact>]
let ``Asech test`` () = Assert.Equal(parsed "asech(x)", asech x)
[<Fact>]
let ``Acsch test`` () = Assert.Equal(parsed "acsch(x)", acsch x)
[<Fact>]
let ``Pow test`` () = Assert.Equal((parsed 2) ** (parsed 5), parsed 32)
[<Fact>]
let ``Sqrt test`` () = Assert.Equal(parsed "sqrt(x)", sqrt x)

[<Fact>]
let ``Sin test exact`` () = Assert.Equal(parsed "0", sin 0)
[<Fact>]
let ``Cos test exact`` () = Assert.Equal(parsed "1", cos 0)
[<Fact>]
let ``Tan test exact`` () = Assert.Equal(parsed "0", tan 0)
[<Fact>]        
let ``Sqrt test exact`` () = Assert.Equal(parsed "22", sqrt 484)
// https://github.com/asc-community/AngouriMath/issues/402 asks for the % operator here as
// well. It needs nothing of its own: F# resolves % through op_Modulus, which is what the
// operator on Entity compiles to, so the kernel's node is what an F# % builds.
[<Fact>]
let ``Mod operator test`` () = Assert.Equal(parsed "x mod 3", x % (parsed 3))
[<Fact>]
let ``Mod operator evaluates`` () = Assert.Equal(parsed 1, (parsed 7 % parsed 3).Evaled)
// The node is the mathematician's mod, so it takes the sign of the divisor and gives 2 here,
// where F#'s own % on two integers would give -1. It is the same operator name over a different
// type, as % over a matrix or a polynomial would be.
[<Fact>]
let ``Mod operator follows the divisor's sign`` () = Assert.Equal(parsed 2, (parsed -7 % parsed 3).Evaled)
