open Functions
open Operators
open FromToString
open AngouriMath

let print x =
    printfn "%O" x

// just built a tree
print ("x" + 2)

// evaluation to another entity
print (evaled "2 + 3 or false")

// evaluation to AM's number
print (as_number "2 + 3")

// evaluation to AM's boolean
print (as_bool "true or false implies true")

// differentiation
print (differentiate "x" "x2 + a x")
print (differentiate "x" "e^x + ln(x) + log(2, x)")
print (differentiate "y" "sin(sin(y))")

// integration
print (integrate "x" "x2 + e")
print (integrate "x" "a x")
print (integrate "x" "a x + b e ^ x + ln(x)")

// limit
print (limit "x" "0" "sin(a x) / x")
print ("sin(a x) / x" &&& "x" --> 0)

// LaTeX
print (latex "x / e + alpha + sqrt(x) + integral(y + 3, y, 1)")
