open Functions

open AngouriMath
open Core
open Constants
open FSharp.Collections
open Operators
open Option
open Compilation
open Shortcuts

let expr = parsed "x + 12y"

let s = substituted "x" 3 expr

printfn "%O" (("y", 243) -|> (("x", 13) -|> expr))

printfn "%O" (("y", 24) -|> expr <|- ("x", 244))

let i = ``int [dx]`` expr

