module AngouriMath.FSharp.MatrixOperators

open AngouriMath
open AngouriMath.FSharp.Core
open AngouriMath.FSharp.Functions

let (|*) a b =
    match (parsed a * parsed b).InnerSimplified with
    | :? Entity.Matrix as m -> m
    | other -> vector [other]

let (|**) a b =
    match (parsed a ** parsed b).InnerSimplified with
    | :? Entity.Matrix as m -> m
    | other -> vector [other]

let (|+) a b =
    match (parsed a + parsed b).InnerSimplified with
    | :? Entity.Matrix as m -> m
    | other -> vector [other]

let (|-) a b =
    match (parsed a - parsed b).InnerSimplified with
    | :? Entity.Matrix as m -> m
    | other -> vector [other]

let (|/) a b =
    match (parsed a / parsed b).InnerSimplified with
    | :? Entity.Matrix as m -> m
    | other -> vector [other]
