module Shortcuts

// Here we will store "overloads" and short forms of functions with some parameters
// being set to default (for example, it might be variable `x` in many cases)

open Functions

/// Finds the derivative of the given expression
/// over x
let ``dy/dx`` expr = differentiate "x" expr

/// Finds the integral of the given expression
let ``int [dx]`` expr = integrate "x" expr

/// Finds the both-sided limit of the given expression
/// for x approaching 0
let ``lim x->0`` expr = limit "x" 0 expr

/// Finds the both-sided limit of the given expression
/// for x approaching positive infinity
let ``lim x->+oo`` expr = limit "x" "+oo" expr

/// Finds the both-sided limit of the given expression
/// for x approaching negative infinity
let ``lim x->-oo`` expr = limit "x" "-oo" expr
