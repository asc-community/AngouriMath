open Functions
open Operators
open FromToString

let print_str x =
    printfn "%s" x

let print x =
    printfn "%s" (to_string x)

let line() =
    printfn "%s" ""

print_str "Alright, let's start from a hello world"
print (evaled (expr "3 + 2"))
line()

print (simplify (expr "x + 3 + 4 + x + 2x + 23 + a"))
line()

let x = symbol "x"
let expression = (x.Sin() ^ (expr "2")) + (x.Cos() ^ expr "2")
print expression
print (simplify expression)
line()

print (simplify (differentiate (expr "x2 + a x + b ^ x") (symbol "x")))
line()

print (set "{ 1, 2 }")
print (simplify (expr @"{ 1, 2 } \/ { a }"))
line()

print ((x => x) &&& x)
line()

print ((expr "sin(x) / x", x) --> (expr "0"))
line()