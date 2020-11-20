module ReturnValues.Continuous

open Core
open MathFunctions.Continuous
open Xunit

let x = symbol "x"

[<Fact>]
let ``Test symbol x`` () = Assert.Equal(parse "x", symbol "x")
[<Fact>]
let ``Test symbol y`` () = Assert.Equal(parse "y", symbol "y")
[<Fact>]
let ``Sin test`` () = Assert.Equal(parse "sin(x)", sin x)
[<Fact>]
let ``Cos test`` () = Assert.Equal(parse "cos(x)", cos x)
[<Fact>]
let ``Tan test`` () = Assert.Equal(parse "tan(x)", tan x)
[<Fact>]
let ``Cot test`` () = Assert.Equal(parse "cot(x)", cot x)
[<Fact>]
let ``Sec test`` () = Assert.Equal(parse "sec(x)", sec x)
[<Fact>]
let ``Csc test`` () = Assert.Equal(parse "csc(x)", csc x)
[<Fact>]
let ``Asin test`` () = Assert.Equal(parse "asin(x)", asin x)
[<Fact>]
let ``Acos test`` () = Assert.Equal(parse "acos(x)", acos x)
[<Fact>]
let ``Atan test`` () = Assert.Equal(parse "atan(x)", atan x)
[<Fact>]
let ``Acot test`` () = Assert.Equal(parse "acot(x)", acot x)
[<Fact>]
let ``Asec test`` () = Assert.Equal(parse "asec(x)", asec x)
[<Fact>]
let ``Acsc test`` () = Assert.Equal(parse "acsc(x)", acsc x)
[<Fact>]
let ``Sinh test`` () = Assert.Equal(parse "sinh(x)", sinh x)
[<Fact>]
let ``Cosh test`` () = Assert.Equal(parse "cosh(x)", cosh x)
[<Fact>]
let ``Tanh test`` () = Assert.Equal(parse "tanh(x)", tanh x)
[<Fact>]
let ``Coth test`` () = Assert.Equal(parse "coth(x)", coth x)
[<Fact>]
let ``Sech test`` () = Assert.Equal(parse "sech(x)", sech x)
[<Fact>]
let ``Csch test`` () = Assert.Equal(parse "csch(x)", csch x)
[<Fact>]
let ``Asinh test`` () = Assert.Equal(parse "asinh(x)", asinh x)
[<Fact>]
let ``Acosh test`` () = Assert.Equal(parse "acosh(x)", acosh x)
[<Fact>]
let ``Atanh test`` () = Assert.Equal(parse "atanh(x)", atanh x)
[<Fact>]
let ``Acoth test`` () = Assert.Equal(parse "acoth(x)", acoth x)
[<Fact>]
let ``Asech test`` () = Assert.Equal(parse "asech(x)", asech x)
[<Fact>]
let ``Acsch test`` () = Assert.Equal(parse "acsch(x)", acsch x)