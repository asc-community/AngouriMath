module Core

open AngouriMath
open PeterO.Numbers
open System.Linq

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

type Lengths =
    | Any
    | Invalid
    | Fixed of int

let rec parseRow = function
| [] -> Some([])
| hd::tl ->
    match parseSilent hd with
    | None -> None
    | Some(parsedHead) -> 
        match parseRow tl with
        | None -> None
        | Some(parsedTail) -> Some(parsedHead::parsedTail)

let matrix x = 
    let rec columnCount (x : 'T list list) =
        match x with
        | [] -> Any
        | hd::tl ->
            match columnCount tl with
            | Any -> Fixed(hd.Length)
            | Invalid -> Invalid
            | Fixed len -> if len = hd.Length then Fixed(len) else Invalid

    let parseListOfLists li =
        let rec build = function
            | [] -> Some([])
            | hd::tl ->
                match build tl with
                | None -> None
                | Some(row) -> 
                    match parseRow hd with
                    | None -> None
                    | Some(good) -> Some(good::row)
        build li

    match parseListOfLists x with
    | None -> raise ParseException
    | Some(good) -> MathS.Matrix(array2D good)


/// Creates a column vector from a 1-dimensional array
let vector (li : List<'T>) =
    match parseRow li with
    | None -> raise ParseException
    | Some(parsed) -> MathS.Vector(parsed.ToArray())

type LimSide =
    | Left
    | Right