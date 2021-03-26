open Functions
open Operators
open AngouriMath
open Core
open Constants
open FSharp.Collections


let m = matrix (array2D [
    [ parse 1; parse 0 ] ;
    [ parse "x"; parse "y" ]
    ])

printfn "%O" (matrix_to_string m)

let v = vector [| parse 2 ; parse 3 |]

printfn "%O" (matrix_to_string v)