[<AutoOpen>]
module Operators

open Core
open AngouriMath
open Functions

let (-|>) (x, v) expr = (substitute (symbol x) (parse v) expr).InnerSimplified

let (<|-) expr (x, v) = (substitute (symbol x) (parse v) expr).InnerSimplified
