open Functions
open Operators
open FromToString
open AngouriMath

let print x =
    printfn "%s" (to_string x)

print ("sin(x) / x" &&& ("x" --> "0"))
print ("a / x" &&& ("x" --> ("0" <-- "left")))