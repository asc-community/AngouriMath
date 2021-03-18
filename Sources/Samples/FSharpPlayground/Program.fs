open Functions
open Operators
open AngouriMath
open Core
open Constants

printfn "%O" (as_number "2 + 3")
printfn "%O" (differentiate "x" "x2 + a x")
printfn "%O" (integrate "x" "x2 + e")
printfn "%O" (limit "x" "0" "sin(a x) / x")
