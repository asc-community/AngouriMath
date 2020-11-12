module Core

open AngouriMath
open PeterO.Numbers

exception ExprParseException of string * obj

let parse (x : obj) =
    match x with
    | :? Entity as e -> e
    | :? string as s -> MathS.FromString(s)
    | :? int as i -> upcast MathS.Numbers.Create(i)
    | :? bool as b -> upcast MathS.Boolean.Create(b)
    | :? double as d -> upcast MathS.Numbers.Create(d)
    | :? decimal as d -> upcast MathS.Numbers.Create(EDecimal.FromDecimal(d))
    | unresolved -> raise (ExprParseException("", unresolved))

let parse_g<'T when 'T :> Entity> x =
    let parsed = parse x
    match parsed with
    | :? 'T as correct -> correct
    | _ -> raise (ExprParseException("Cannot parse ", x))

let parse_symbol x =
    parse_g<Entity.Variable> x