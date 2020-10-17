module FromToString
open Core

open AngouriMath
open AngouriMath.Core.Exceptions

let latex (x : Entity) =
    x.Latexise()
    
let to_string (expr : Entity) =
    expr.ToString()

let symbol x =
    parse_g<Entity.Variable> x

let set x =
    parse_g<Entity.Set> x

