open Functions
open Operators
open AngouriMath
open Core
open Constants
open FSharp.Collections
open Option


let m = matrix ([
    [ "1"; "0" ] ;
    [ "x"; "y" ]
    ])

printfn "%O" (matrixToString m)

let v = vector [ 2 ; 3 ]

printfn "%O" (matrixToString v)

printfn "%O" (matrixToString (m * v))

let n = modifiedMatrix m (fun r c e -> 
    match (r, c, e) with
    | (0, 1, _) -> (parse 3)
    | _ -> e
    )

printfn "%O" (matrixToString n)

printfn "%O" (matrixToString (transposed n))

// printfn "%O" (matrixToString(matrix "[ 1, 2, 3 ]"))

printfn "%O" (vector [ 2; 3 ])

//      
// let modifier r c (e: AngouriMath.Entity) =
//     match (r, c, e) with
//     | (0, 1, _) -> (parse 3)
//     | _ -> e
//     

// let n = modified_matrix m modifier
