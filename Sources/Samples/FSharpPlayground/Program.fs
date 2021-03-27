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

printfn "%O" (matrixToString m)

let v = vector [| parse 2 ; parse 3 |]

printfn "%O" (matrixToString v)

printfn "%O" (matrixToString (m * v))

let n = modifiedMatrix m (fun r c e -> 
    match (r, c, e) with
    | (0, 1, _) -> (parse 3)
    | _ -> e
    )

printfn "%O" (matrixToString n)

printfn "%O" (matrixToString (transposed n))

printfn "%O" (matrixToString(matrix "[ 1, 2, 3 ]"))

printfn "%O" (vector [| parse 2, parse 3 |])

//      
// let modifier r c (e: AngouriMath.Entity) =
//     match (r, c, e) with
//     | (0, 1, _) -> (parse 3)
//     | _ -> e
//     

// let n = modified_matrix m modifier
