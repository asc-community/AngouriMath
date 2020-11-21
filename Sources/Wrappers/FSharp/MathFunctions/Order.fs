module MathFunctions.Order

open Core
open AngouriMath

let derivative x expr = MathS.Derivative(parse expr, parse x)

let integral x expr = MathS.Integral(parse expr, parse x)

let limited x dest expr = MathS.Limit(parse expr, parse x, parse dest)

let limited_sided x dest expr side =
    match side with
    | Left -> MathS.Limit(parse expr, parse x, parse dest, AngouriMath.Core.ApproachFrom.Left)
    | Right -> MathS.Limit(parse expr, parse x, parse dest, AngouriMath.Core.ApproachFrom.Right)