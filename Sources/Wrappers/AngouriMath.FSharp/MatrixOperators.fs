module AngouriMath.FSharp.MatrixOperators

open AngouriMath
open AngouriMath.FSharp.Core
open AngouriMath.FSharp.Functions

let (|*) a b = (parsed a * parsed b).InnerSimplified |> asMatrix

let (|**) a b = (parsed a ** parsed b).InnerSimplified |> asMatrix

let (|+) a b = (parsed a + parsed b).InnerSimplified |> asMatrix

let (|-) a b = (parsed a - parsed b).InnerSimplified |> asMatrix

let (|/) a b = (parsed a / parsed b).InnerSimplified |> asMatrix