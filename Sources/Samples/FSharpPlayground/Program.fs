open Core
open Functions
open MathFunctions.Continuous
open MathFunctions.Order
open MathFunctions.Discrete
open Constants
open AngouriMath

let x = symbol "x"
// let expr = x * y + (sin x) * (cos x) + pow e x
let expr = sin(x) / cos(x) - sqr(x) - integral x x

printfn "%O" expr

let a = (equal x (x + x))

