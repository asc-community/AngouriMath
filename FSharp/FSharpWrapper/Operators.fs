[<AutoOpen>]
module Operators

open AngouriMath

let (^) (base_ : Entity) (power : Entity) =
    base_.Pow(power)

let (@?) (expr : Entity) x =
    expr.Solve(x)

let (&&&) (left : Entity) (right : Entity) =
    new Entity.Andf(left, right)

let (|||) (left : Entity) (right : Entity) =
    new Entity.Orf(left, right)

let (=>) (left : Entity) (right : Entity) =
    new Entity.Impliesf(left, right)

let (-->) (expr : Entity, var : Entity.Variable) (dest : Entity) =
    expr.Limit(var, dest)