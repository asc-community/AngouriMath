module Functions

open Core

/// simplifies the given expression
let simplify x =
    (parse x).Simplify()

/// Computes the derivative of the given variable and expression
let differentiate x expr =
    (parse expr).Differentiate(parse_symbol x)

let diff x expr = differentiate x expr

/// Computes the integral of the given variable and expression
let integrate x expr = 
    (parse expr).Integrate(parse_symbol x)

/// Computes the limit of the given variable, destination to where it approaches, and expression
let limit x destination expr =
    (parse expr).Limit(parse_symbol x, parse destination)

/// Computes the limit of the given variable, destination to where it approaches, expression itself, and the origin side
let limitSided x destination expr side =
    match side with
    | Left -> (parse expr).Limit(parse_symbol x, parse destination, AngouriMath.Core.ApproachFrom.Left)
    | Right -> (parse expr).Limit(parse_symbol x, parse destination, AngouriMath.Core.ApproachFrom.Right)

/// Takes the evaluated form of an expression (preserving the type)
let evaled expr =
    (parse expr).Evaled

/// Takes the evaluated form of an expression (as a number; if it cannot, an exception is thrown)
let asNumber expr =
    (parse expr).EvalNumerical()

/// Takes the evaluated form of an expression (as a boolean; if it cannot, an exception is thrown)
let asBool expr =
    (parse expr).EvalBoolean()

/// Solves the statement/predicate over the given variable
let solve x expr =
    (parse expr).Solve(parse_symbol x)

/// Gets the LaTeX form of an expression
let latex x =
    (parse x).Latexise()

/// Substitutes the given variable with the given value in the given expression
let substitute x value expr =
    (parse expr).Substitute(parse x, parse value)

/// Returns a multiline string representation of matrix
let matrixToString (x: AngouriMath.Entity.Matrix) = x.ToString(true)

/// Returns a matrix modified according to the modifier
let modifiedMatrix (x: AngouriMath.Entity.Matrix) m =
    x.With(new System.Func<int, int, AngouriMath.Entity, AngouriMath.Entity>(m))

/// Gets the transposed form of a matrix or vector
let transposed (m: AngouriMath.Entity.Matrix) = m.T
