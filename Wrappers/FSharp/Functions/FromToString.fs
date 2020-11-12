module FromToString
open Core

open AngouriMath
open AngouriMath.Core.Exceptions

let latex (x : obj) =
    (parse x).Latexise()

let symbol x =
    parse_symbol x

let set x =
    parse_g<Entity.Set> x

