module Functions

open Core

let simplify x =
    (parse x).Simplify()

let differentiate expr x =
    (parse x).Derive(x)

let integrate expr x = 
    (parse expr).Integrate(x)

let limit expr x destination =
    (parse expr).Limit(x, destination)

let limit_sided expr x destination side =
    (parse expr).Limit(x, destination, side)

let evaled expr =
    (parse expr).Evaled

let as_number expr =
    (parse expr).EvalNumerical()

let as_bool expr =
    (parse expr).EvalNumerical()

let solve expr x =
    (parse expr).Solve(x)
