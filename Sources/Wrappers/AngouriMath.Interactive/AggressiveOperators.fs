module AngouriMath.Interactive.AggressiveOperators

open AngouriMath.FSharp.Core
open AngouriMath.FSharp.Functions

let ( + ) a b =
    ((parsed a) + (parsed b)).InnerSimplified
let ( - ) a b =
    ((parsed a) - (parsed b)).InnerSimplified
let ( * ) a b =
    ((parsed a) * (parsed b)).InnerSimplified
let ( / ) a b =
    ((parsed a) / (parsed b)).InnerSimplified
let ( ** ) a b =
    ((parsed a).Pow(parsed b)).InnerSimplified
let ( ~~~ ) a = 
    (negation (parsed a)).InnerSimplified
let ( ||| ) a b = 
    (union (parsed a) (parsed b)).InnerSimplified
let ( &&& ) a b = 
    (intersect (parsed a) (parsed b)).InnerSimplified
let ( = ) a b = 
    (equality (parsed a) (parsed b)).InnerSimplified
let ( <> ) a b = 
    (negation (equality (parsed a) (parsed b))).InnerSimplified
let ( > ) a b = 
    (greater (parsed a) (parsed b)).InnerSimplified
let ( < ) a b = 
    (less (parsed a) (parsed b)).InnerSimplified
let ( >= ) a b = 
    (greaterOrEqual (parsed a) (parsed b)).InnerSimplified  
let ( <= ) a b = 
    (lessOrEqual (parsed a) (parsed b)).InnerSimplified
