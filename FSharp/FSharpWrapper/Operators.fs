[<AutoOpen>]
module Operators

open Core
open FromToString
open AngouriMath

exception UnrecognizedDirection of string

type Tending = { var: obj; destination: obj; }

let (@?) expr x =
    (parse expr).Solve(symbol x)

let (&&&) left (right : obj) =
    let expr = parse left
    match right with
    | :? Tending as tending -> expr.Limit(parse_g<Entity.Variable> tending.var, parse tending.destination)
    | _ -> MathS.Conjunction(expr, parse right)

let (|||) left right =
    MathS.Disjunction(parse left, parse right)

let (=>) assumption conclusion =
    (parse assumption).Implies(parse conclusion)

let (-->) x destination =
    { var = x; destination = destination; }

let (^) base_ power =
    (parse base_).Pow(parse power)

let (+) a b =
    (parse a) + (parse b)

let (-) a b =
    (parse a) - (parse b)

let (/) a b =
    (parse a) / (parse b)

let (*) a b =
    (parse a) * (parse b)