module AngouriMath.FSharp.Shortcuts

// Here we will store "overloads" and short forms of functions with some parameters
// being set to default (for example, it might be variable `x` in many cases)

open Functions
open Core

/// Finds the derivative of the given expression
/// over x
let ``d/dx`` expr = derivative "x" expr

/// Finds the integral of the given expression
let ``int [dx]`` expr = integral "x" expr

/// Finds the both-sided limit of the given expression
/// for x approaching 0
let ``lim x->0`` expr = limit "x" 0 expr

/// Finds the both-sided limit of the given expression
/// for x approaching positive infinity
let ``lim x->+oo`` expr = limit "x" "+oo" expr

/// Finds the both-sided limit of the given expression
/// for x approaching negative infinity
let ``lim x->-oo`` expr = limit "x" "-oo" expr

/// Substitutes the first argument from the tuple with the second
/// argument in the tuple in the given expression
let (-|>) (x, v) expr = (substituted (symbol x) (parsed v) expr).InnerSimplified

/// In the given expresion, substitutes the first argument from the tuple
/// with the second argument from the tuple
let (<|-) expr (x, v) = (substituted (symbol x) (parsed v) expr).InnerSimplified
