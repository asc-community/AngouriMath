[<AutoOpen>]
module Operators

open Core
open FromToString
open AngouriMath

exception UnrecognizedDirection of string

type Sided =
    | Left of obj
    | Right of obj

type Tending =
    | TendingTo of obj * obj
    | TendingToFrom of x : obj * side : Sided

let (^) base_ power =
    (parse base_).Pow(parse power)

let (@?) expr x =
    (parse expr).Solve(symbol x)

let (&&&) (left : obj) (right : obj) =
    let expr = parse left
    match right with
    | :? Tending as tending -> 
        match tending with
        | TendingTo(x, dest) -> expr.Limit(parse_g<Entity.Variable> x, parse dest)
        | TendingToFrom(x, dest) ->
            match dest with
            | Right dest -> expr.Limit(parse_g<Entity.Variable> x, parse dest, Core.ApproachFrom.Right)
            | Left dest -> expr.Limit(parse_g<Entity.Variable> x, parse dest, Core.ApproachFrom.Left)
    | _ -> MathS.Conjunction(expr, parse right)

let (|||) left right =
    MathS.Disjunction(parse left, parse right)

let (=>) assumption conclusion =
    (parse assumption).Implies(parse conclusion)

let (-->) x (destination : obj) =
    match destination with
    | :? Sided as sided -> TendingToFrom(x, sided)
    | _ -> TendingTo(x, destination)
    
let (<--) dst side =
    match side with
    | "left" | "-" -> Left(dst)
    | "right" | "+" -> Right(dst)
    | _ -> raise (UnrecognizedDirection(side))
    

let (+) a b =
    (parse a) + (parse b)
