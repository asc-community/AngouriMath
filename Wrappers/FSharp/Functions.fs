module Functions

open Core

/// simplifies the given expression
let simplify x =
    (parse x).Simplify()

/// Computes the derivative of the given variable and expression
let differentiate x expr =
    (parse expr).Differentiate(parse_symbol x)

let diff x expr = differentiate x expr

/// Computes the integral of the given variable and expression
let integrate x expr = 
    (parse expr).Integrate(parse_symbol x)

/// Computes the limit of the given variable, destination to where it approaches, and expression
let limit x destination expr =
    (parse expr).Limit(parse_symbol x, parse destination)

/// Computes the limit of the given variable, destination to where it approaches, expression itself, and the origin side
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
