module Functions

open Core

type LimSide =
    | Left
    | Right

let simplify x =
    (parse x).Simplify()

let differentiate x expr =
    (parse expr).Differentiate(parse_symbol x)

let diff x expr = differentiate x expr

let integrate x expr = 
    (parse expr).Integrate(parse_symbol x)

let limit x destination expr =
    (parse expr).Limit(parse_symbol x, parse destination)

let lim x destination expr = limit x destination expr

let limit_sided x destination expr side =
    match side with
    | Left -> (parse expr).Limit(parse_symbol x, parse destination, AngouriMath.Core.ApproachFrom.Left)
    | Right -> (parse expr).Limit(parse_symbol x, parse destination, AngouriMath.Core.ApproachFrom.Right)

let evaled expr =
    (parse expr).Evaled

let as_number expr =
    (parse expr).EvalNumerical()

let as_bool expr =
    (parse expr).EvalBoolean()

let solve x expr =
    (parse expr).Solve(parse_symbol x)

let latex x =
    (parse x).Latexise()

let substitute x value expr =
    (parse expr).Substitute(parse x, parse value)
