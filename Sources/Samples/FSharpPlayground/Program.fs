open Functions

open AngouriMath
open Core
open Constants
open FSharp.Collections
open Operators
open Option
open Compilation
open Shortcuts

let expr = parse "x + 12"

printfn "%O" (("x", 13) -|> expr)
