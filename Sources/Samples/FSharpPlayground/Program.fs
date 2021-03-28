open Functions

open AngouriMath
open Core
open Constants
open FSharp.Collections
open Operators
open Option
open Compilation
open Shortcuts

let expr = parse "x + 12y"

printfn "%O" (("y", 243) -|> (("x", 13) -|> expr))
printfn "%O" (("y", 24) -|> expr <|- ("x", 244))
