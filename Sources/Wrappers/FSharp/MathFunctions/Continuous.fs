module MathFunctions.Continuous

open Core
open AngouriMath

let log log_base x = MathS.Log(parse log_base, parse x)

let ln x = MathS.Ln(parse x)

let sqr x = MathS.Sqr(parse x)

let sqrt x = MathS.Sqrt(parse x)

let cbrt x = MathS.Cbrt(parse x)

let pow x power = MathS.Pow(parse x, parse power)

let factorial x = MathS.Factorial(parse x)

let gamma x = MathS.Gamma(parse x)

let sgn x = MathS.Signum(parse x)

let sin x = (parse x).Sin()

let cos x = (parse x).Cos()

let tan x = (parse x).Tan()

let cot x = (parse x).Cotan()

let sec x = (parse x).Sec()

let csc x = (parse x).Cosec()

let asin x = (parse x).Arcsin()

let acos x = (parse x).Arccos()

let atan x = (parse x).Arctan()

let acot x = (parse x).Arccotan()

let asec x = (parse x).Arcsec()

let acsc x = (parse x).Arccosec()

let sinh x = MathS.Hyperbolic.Sinh(parse x)

let cosh x = MathS.Hyperbolic.Cosh(parse x)

let tanh x = MathS.Hyperbolic.Tanh(parse x)

let coth x = MathS.Hyperbolic.Cotanh(parse x)

let sech x = MathS.Hyperbolic.Sech(parse x)

let csch x = MathS.Hyperbolic.Cosech(parse x)

let asinh x = MathS.Hyperbolic.Arsinh(parse x)

let acosh x = MathS.Hyperbolic.Arcosh(parse x)

let atanh x = MathS.Hyperbolic.Artanh(parse x)

let acoth x = MathS.Hyperbolic.Arcotanh(parse x)

let asech x = MathS.Hyperbolic.Arsech(parse x)

let acsch x = MathS.Hyperbolic.Arcosech(parse x)