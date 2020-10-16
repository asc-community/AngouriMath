module Functions

open AngouriMath

let expr x =
    MathS.FromString(x)

let parse x =
    MathS.FromString(x)

let simplify (x : Entity) =
    x.Simplify()

let differentiate (expr : Entity) x =
    expr.Derive(x)

let integrate (expr : Entity) x = 
    expr.Integrate(x)

let limit (expr : Entity) x destination =
    expr.Limit(x, destination)

let limit_sided (expr : Entity) x destination side =
    expr.Limit(x, destination, side)

let evaled (expr : Entity) =
    expr.Evaled

let as_number (expr : Entity) =
    expr.EvalNumerical()

let as_bool (expr : Entity) =
    expr.EvalNumerical()

let solve ( expr : Entity ) x =
    expr.Solve(x)

type Entity with
    member Entity.Sin x =
        MathS.Sin(x)
