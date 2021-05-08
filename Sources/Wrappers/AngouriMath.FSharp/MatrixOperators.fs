module AngouriMath.FSharp.MatrixOperators

open AngouriMath
open AngouriMath.FSharp.Core
open AngouriMath.FSharp.Functions

/// If the provided entity is a matrix,
/// it is returned downcasted. Otherwise,
/// a 1x1 matrix containing the entity is returned.
let asMatrix a = 
    match parsed a with
    | :? Entity.Matrix as m -> m
    | other -> vector [other]

let (|*) a b = (parsed a * parsed b).InnerSimplified |> asMatrix

let (|**) a b = (parsed a ** parsed b).InnerSimplified |> asMatrix

let (|+) a b = (parsed a + parsed b).InnerSimplified |> asMatrix

let (|-) a b = (parsed a - parsed b).InnerSimplified |> asMatrix

let (|/) a b = (parsed a / parsed b).InnerSimplified |> asMatrix