module Functions

open Core

printfn "Quack"

type LimSide =
    | Left
    | Right

let simplify x =
    (parse x).Simplify()

let differentiate x expr =
    (parse expr).Differentiate(parse_symbol x)

let integrate x expr = 
    (parse expr).Integrate(parse_symbol x)

let limit x destination expr =
    (parse expr).Limit(parse_symbol x, parse destination)

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


let latex (x : obj) =
    (parse x).Latexise()