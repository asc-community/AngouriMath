open Functions
open Operators
open Shortcuts


printfn "%O" (solutions "x" "x + 2 = 0")

printfn "%O" (simplified (solutions "x" "x2 + 2 a x + a2 = 0"))

printfn "%O" (``dy/dx`` "x2 + a x")

printfn "%O" (integral "x" "x2 + e")

printfn "%O" (``lim x->0`` "sin(a x) / x")

printfn "%O" (latex "x / e + alpha + sqrt(x) + integral(y + 3, y, 1)")
