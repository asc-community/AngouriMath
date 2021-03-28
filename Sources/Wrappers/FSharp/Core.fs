module Core

open AngouriMath
open PeterO.Numbers

exception ParseException

/// Parses the given object (be that string, number, or something else)
/// into an Entity. In case it cannot, an exception is thrown
let parseSilent (x : obj) =
    match x with
    | :? Entity as e -> Some(e)
    | :? string as s -> Some(MathS.FromString(s))
    | :? int as i -> Some(upcast MathS.Numbers.Create(i) : Entity)
    | :? bool as b -> Some(upcast MathS.Boolean.Create(b) : Entity)
    | :? double as d -> Some(upcast MathS.Numbers.Create(d) : Entity)
    | :? decimal as d -> Some(upcast MathS.Numbers.Create(EDecimal.FromDecimal(d)) : Entity)
    | unresolved -> None

let parse s =
    match parseSilent s with
    | None -> raise ParseException
    | Some(res) -> res

/// Parses into arbitrary type
let parseTypeSilent<'T when 'T :> Entity> x =
    match parseSilent x with
    | None -> None
    | Some(v) ->
        match v with
        | :? 'T as correct -> Some(correct)
        | _ -> None
   
let parseType x =
    match parseTypeSilent x with
    | None -> raise ParseException
    | Some(v) -> v

/// Parses into a variable
let parseSymbol x : Entity.Variable = parseType x

/// Creates a variable from string
let symbol x = parseSymbol x
    
/// Creates a set from string
let set x = parseTypeSilent<Entity.Set> x

/// The first argument is the setting to change
/// The second argument is the new value of the setting
/// The third argument is what to execute under
/// the selected settings.
let withSetting (setting: AngouriMath.Convenience.Setting<'T>) newValue f =
    let unit = setting.Set(newValue)
    let res = f()
    unit.Dispose()
    res