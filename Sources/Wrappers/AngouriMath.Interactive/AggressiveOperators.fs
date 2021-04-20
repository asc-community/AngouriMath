module AngouriMath.Interactive.AggressiveOperators

open Core

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