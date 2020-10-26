open Functions
open Operators
open FromToString
open AngouriMath
open Core
open Constants

let print x =
    printfn "%O" x

let line() = printfn "%s" ""

type StringOrEntity =
    | AsString of string
    | AsEntity of Entity

// just built a tree
print "x + 2"
line()

// evaluation to another entity
print (evaled "2 + 3 or false")
line()

// evaluation to AM's number
print (as_number "2 + 3")
line()

// evaluation to AM's boolean
print (as_bool "true or false implies true")
line()

// solving
print (solve "x" "x + 2 = 0")
print (simplify (solve "x" "x2 + 2 a x + a2 = 0"))
print (solve "x" "(x - 3)(x + a) = 0 and (x - 3)(x + 3) = 0 or x > a")
//print (solve "x" "(x - 3)(x + a) = 0")
line()

// differentiation
print (differentiate "x" "x2 + a x")
print (differentiate "x" "e^x + ln(x) + log(2, x)")
print (differentiate "y" "sin(sin(y))")
line()

// integration
print (integrate "x" "x2 + e")
print (integrate "x" "a x")
print (integrate "x" "a x + b e ^ x + ln(x)")
line()

// limit
print (limit "x" "0" "sin(a x) / x")
print ((parse "sin(a x) / x") &&&& ((parse "x") --> zero))
line()

// LaTeX
print (latex "x / e + alpha + sqrt(x) + integral(y + 3, y, 1)")
line()