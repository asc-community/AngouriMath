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
let ( ^^^ ) a b = 
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
let sin a = 
    (sin (parsed a)).InnerSimplified
let cos a = 
    (cos (parsed a)).InnerSimplified
let tan a = 
    (tan (parsed a)).InnerSimplified
let asin a = 
    (asin (parsed a)).InnerSimplified
let acos a = 
    (acos (parsed a)).InnerSimplified
let atan a = 
    (atan (parsed a)).InnerSimplified
let sinh a = 
    (sinh (parsed a)).InnerSimplified
let cosh a = 
    (cosh (parsed a)).InnerSimplified
let tanh a = 
    (tanh (parsed a)).InnerSimplified
let sqrt a = 
    (sqrt (parsed a)).InnerSimplified
let log logbase a = 
    (log (parsed logbase) (parsed a)).InnerSimplified
let log10 a = 
    (log (parsed "10") (parsed a)).InnerSimplified
let ln a = 
    (ln (parsed a)).InnerSimplified
