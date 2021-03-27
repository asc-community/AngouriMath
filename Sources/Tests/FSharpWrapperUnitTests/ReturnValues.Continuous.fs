module ReturnValues.Continuous

open Core
open MathFunctions.Continuous
open Xunit

let x = symbol "x"

[<Fact>]
let ``Test symbol x`` () = Assert.Equal(parseSilent "x", symbol "x")
[<Fact>]
let ``Test symbol y`` () = Assert.Equal(parseSilent "y", symbol "y")
[<Fact>]
let ``Sin test`` () = Assert.Equal(parseSilent "sin(x)", sin x)
[<Fact>]
let ``Cos test`` () = Assert.Equal(parseSilent "cos(x)", cos x)
[<Fact>]
let ``Tan test`` () = Assert.Equal(parseSilent "tan(x)", tan x)
[<Fact>]
let ``Cot test`` () = Assert.Equal(parseSilent "cot(x)", cot x)
[<Fact>]
let ``Sec test`` () = Assert.Equal(parseSilent "sec(x)", sec x)
[<Fact>]
let ``Csc test`` () = Assert.Equal(parseSilent "csc(x)", csc x)
[<Fact>]
let ``Asin test`` () = Assert.Equal(parseSilent "asin(x)", asin x)
[<Fact>]
let ``Acos test`` () = Assert.Equal(parseSilent "acos(x)", acos x)
[<Fact>]
let ``Atan test`` () = Assert.Equal(parseSilent "atan(x)", atan x)
[<Fact>]
let ``Acot test`` () = Assert.Equal(parseSilent "acot(x)", acot x)
[<Fact>]
let ``Asec test`` () = Assert.Equal(parseSilent "asec(x)", asec x)
[<Fact>]
let ``Acsc test`` () = Assert.Equal(parseSilent "acsc(x)", acsc x)
[<Fact>]
let ``Sinh test`` () = Assert.Equal(parseSilent "sinh(x)", sinh x)
[<Fact>]
let ``Cosh test`` () = Assert.Equal(parseSilent "cosh(x)", cosh x)
[<Fact>]
let ``Tanh test`` () = Assert.Equal(parseSilent "tanh(x)", tanh x)
[<Fact>]
let ``Coth test`` () = Assert.Equal(parseSilent "coth(x)", coth x)
[<Fact>]
let ``Sech test`` () = Assert.Equal(parseSilent "sech(x)", sech x)
[<Fact>]
let ``Csch test`` () = Assert.Equal(parseSilent "csch(x)", csch x)
[<Fact>]
let ``Asinh test`` () = Assert.Equal(parseSilent "asinh(x)", asinh x)
[<Fact>]
let ``Acosh test`` () = Assert.Equal(parseSilent "acosh(x)", acosh x)
[<Fact>]
let ``Atanh test`` () = Assert.Equal(parseSilent "atanh(x)", atanh x)
[<Fact>]
let ``Acoth test`` () = Assert.Equal(parseSilent "acoth(x)", acoth x)
[<Fact>]
let ``Asech test`` () = Assert.Equal(parseSilent "asech(x)", asech x)
[<Fact>]
let ``Acsch test`` () = Assert.Equal(parseSilent "acsch(x)", acsch x)