open Core
open Functions
open MathFunctions.Continuous
open Constants

let x, y = symbol "x", symbol "y"
let expr = x * y + (sin x) * (cos x) + pow e x

printfn "%O" (parse "acosh(x)")
