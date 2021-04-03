[<AutoOpen>]
module Operators

open Core
open AngouriMath

type Tending = { var: Entity; destination: Entity; }

let (@?) (expr : Entity) x = expr.Solve x

let (&&&&) (expr : Entity) (right : obj) =
    match right with
    | :? Tending as tending -> expr.Limit(parse_symbol tending.var, parse tending.destination)
    | _ -> MathS.Conjunction(expr, parse right)

let (|||) left right = MathS.Disjunction(left, right)

let (=>) (assumption : Entity) conclusion = assumption.Implies(conclusion)

let (-->) x destination = { var = x; destination = destination; }

let (^) (base_ : Entity) (power : Entity) = base_.Pow power