module Core

open AngouriMath
open PeterO.Numbers
open AngouriMath.Convenience
open System.Linq

exception ExprParseException of string * obj

/// Parses the given object (be that string, number, or something else)
/// into an Entity. In case it cannot, an exception is thrown
let parse (x : obj) =
    match x with
    | :? Entity as e -> e
    | :? string as s -> MathS.FromString(s)
    | :? int as i -> upcast MathS.Numbers.Create(i)
    | :? bool as b -> upcast MathS.Boolean.Create(b)
    | :? double as d -> upcast MathS.Numbers.Create(d)
    | :? decimal as d -> upcast MathS.Numbers.Create(EDecimal.FromDecimal(d))
    | unresolved -> raise (ExprParseException("", unresolved))

/// Parses into arbitrary type
let parse_g<'T when 'T :> Entity> x =
    let parsed = parse x
    match parsed with
    | :? 'T as correct -> correct
    | _ -> raise (ExprParseException("Cannot parse ", x))
   
/// Parses into a variable
let parse_symbol x =
    parse_g<Entity.Variable> x

/// Creates a variable from string
let symbol x =
    parse_symbol x
    
/// Creates a set from string
let set x =
    parse_g<Entity.Set> x

/// Creates a rectangular matrix from a 2-dimensional array
let matrix (x : obj) =
    match x with
    | :? 'EntityArr as arr -> MathS.Matrix(arr)
    | _ -> parse_g<AngouriMath.Entity.Matrix>(x)

/// Creates a column vector from a 1-dimensional array
let vector (li : List<'T>) = MathS.Vector([for el in li do yield (parse el)].ToArray())

let m ad = Array2D.init 2 3

type LimSide =
    | Left
    | Right