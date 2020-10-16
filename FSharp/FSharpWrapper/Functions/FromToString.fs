module FromToString

open AngouriMath
open AngouriMath.Core.Exceptions

let latex (x : Entity) =
    x.Latexise()
    
let to_string (expr : Entity) =
    expr.ToString()

let symbol x =
    MathS.Var(x)

let set x =
    match MathS.FromString(x) with
    | :? Entity.Set as set -> set
    | _ -> raise (new ParseException($"Cannot parse set from {x}"))


        