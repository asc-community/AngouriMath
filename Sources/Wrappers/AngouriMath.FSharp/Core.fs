module AngouriMath.FSharp.Core

open AngouriMath
open PeterO.Numbers

exception ParseException

/// Parses the given object (be that string, number, or something else)
/// into an Entity. In case it cannot, an exception is thrown
let parsedSilent (x : obj) =
    match x with
    | :? Entity as e -> Some(e)
    | :? string as s -> Some(MathS.FromString(s))
    | :? int as i -> Some(upcast MathS.Numbers.Create(i) : Entity)
    | :? bool as b -> Some(upcast MathS.Boolean.Create(b) : Entity)
    | :? double as d -> Some(upcast MathS.Numbers.Create(d) : Entity)
    | :? decimal as d -> Some(upcast MathS.Numbers.Create(EDecimal.FromDecimal(d)) : Entity)
    | unresolved -> None

let parsed s =
    match parsedSilent s with
    | None -> raise ParseException
    | Some(res) -> res

/// Parses into arbitrary type
let parsedTypeSilent<'T when 'T :> Entity> x =
    match parsedSilent x with
    | None -> None
    | Some(v) ->
        match v with
        | :? 'T as correct -> Some(correct)
        | _ -> None
   
let parsedType x =
    match parsedTypeSilent x with
    | None -> raise ParseException
    | Some(v) -> v

/// Creates a variable from string
let symbol x : Entity.Variable = parsedType x
    
/// Creates a set from string
let set x = parsedTypeSilent<Entity.Set> x

/// Returns nodes (subexpressions) of the given expression
let nodesOf expr =
    (parsed expr).Nodes

/// The first argument is the setting to change
/// The second argument is the new value of the setting
/// The third argument is what to execute under
/// the selected settings.
let withSetting (setting: AngouriMath.Convenience.Setting<'T>) newValue f =
    use unit = setting.Set(newValue)
    f()
