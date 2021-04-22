open AngouriMath.FSharp.Core
open AngouriMath.FSharp.Functions
open AngouriMath.FSharp.Shortcuts

// build an expression manually
let x = symbol "x"
let y = symbol "y"
let expr = x / y + x * y
printfn "%O" expr

// just print a parsed expression
printfn "%O" (parsed "x + 2")

// substitute a value
printfn "%O" (("x", 3) -|> "x + 2")

// evaluation to another entity
printfn "%O" (evaled "2 + 3 > 0 or false")

// evaluation to AM's number
printfn "%O" (asNumber "2 + 3")

// evaluation to AM's boolean
printfn "%O" (asBool "true or false implies true")

// solving
printfn "%O" (solutions "x" "x + 2 = 0")
printfn "%O" (simplified (solutions "x" "x2 + 2 a x + a2 = 0"))
printfn "%O" (solutions "x" "(x - 3)(x + a) = 0 and (x - 3)(x + 3) = 0 or x > a")

// differentiation
printfn "%O" (``d/dx`` "x2 + a x")
printfn "%O" (``d/dx`` "e^x + ln(x) + log(2, x)")
printfn "%O" (derivative "y" "sin(sin(y))")

// integration
printfn "%O" (integral "x" "x2 + e")
printfn "%O" (integral "x" "a x")
printfn "%O" (integral "x" "a x + b e ^ x + ln(x)")

// limit
printfn "%O" (``lim x->0`` "sin(a x) / x")
printfn "%O" (limit "x" 0 "sin(a x) / x")

// LaTeX
printfn "%O" (latex "x / e + alpha + sqrt(x) + integral(y + 3, y, 1)")
