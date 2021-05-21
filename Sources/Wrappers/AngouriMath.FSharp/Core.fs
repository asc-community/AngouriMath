module AngouriMath.FSharp.Core

open AngouriMath
open System
open System.Numerics

exception ParseException

/// Returns a parsed expression from an arbitrary type (be that string, number, or something else)
/// into an Entity. In case it cannot, a None is returned.
let parsedSilent (x : obj) =
    match x with
    | :? Entity as e -> Some(e)
    | :? string as s -> Some(Entity.op_Implicit(s))
    | :? Byte  as i ->  Some(Entity.op_Implicit(i))
    | :? SByte as i ->  Some(Entity.op_Implicit(i))
    | :? Int16  as i -> Some(Entity.op_Implicit(i))
    | :? UInt16 as i -> Some(Entity.op_Implicit(i))
    | :? Int32  as i -> Some(Entity.op_Implicit(i))
    | :? UInt32 as i -> Some(Entity.op_Implicit(i))
    | :? Int64  as i -> Some(Entity.op_Implicit(i))
    | :? UInt64 as i -> Some(Entity.op_Implicit(i))
    | :? bool as b -> Some(Entity.op_Implicit(b))
    | :? double as d -> Some(Entity.op_Implicit(d))
    | :? decimal as d -> Some(Entity.op_Implicit(d))
    | unresolved -> None

/// Returns a parsed expression from an arbitrary type (be that string, number, or something else)
/// into an Entity. In case it cannot, an exception is thrown.
let parsed s =
    match parsedSilent s with
    | None -> raise ParseException
    | Some(res) -> res

/// Returns a parsed expression from an arbitrary type (be that string, number, or something else)
/// into an arbitrary type derived from Entity. In case it cannot, a None is returned.
let parsedTypeSilent<'T when 'T :> Entity> x =
    match parsedSilent x with
    | None -> None
    | Some(v) ->
        match v with
        | :? 'T as correct -> Some(correct)
        | _ -> None
  
/// Returns a parsed expression from an arbitrary type (be that string, number, or something else)
/// into an arbitrary type derived from Entity. In case it cannot, an exception is thrown.
let parsedType x =
    match parsedTypeSilent x with
    | None -> raise ParseException
    | Some(v) -> v

/// Creates a variable from string
let symbol (x : obj) : Entity.Variable = parsedType x
    
/// Creates a variable with index from string
let symbolIndexed x index : Entity.Variable = parsedType (x.ToString() + "_" + index.ToString())

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
