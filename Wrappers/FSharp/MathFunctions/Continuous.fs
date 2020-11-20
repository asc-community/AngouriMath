module MathFunctions

open Core
open AngouriMath

let log log_base x = MathS.Log(parse log_base, parse x)

let ln x = MathS.Ln(parse x)

let sqr x = MathS.Sqr(parse x)

let sqrt x = MathS.Sqrt(parse x)

let cbrt x = MathS.Cbrt(parse x)

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

let sinh x = MathS.TrigonometricHyperpolic.Sinh(parse x)

let cosh x = MathS.TrigonometricHyperpolic.Cosh(parse x)

let tanh x = MathS.TrigonometricHyperpolic.Tanh(parse x)

let coth x = MathS.TrigonometricHyperpolic.Cotanh(parse x)

let sech x = MathS.TrigonometricHyperpolic.Sech(parse x)

let csch x = MathS.TrigonometricHyperpolic.Arccosech(parse x)

let asinh x = MathS.TrigonometricHyperpolic.Arcsinh(parse x)

let acosh x = MathS.TrigonometricHyperpolic.Arccosech(parse x)

let atanh x = MathS.TrigonometricHyperpolic.Arctanh(parse x)

let acoth x = MathS.TrigonometricHyperpolic.Arccotanh(parse x)

let asech x = MathS.TrigonometricHyperpolic.Arcsech(parse x)

let acsch x = MathS.TrigonometricHyperpolic.Arccosech(parse x)