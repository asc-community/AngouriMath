module Functions

open Core
open AngouriMath

/// simplifies the given expression
let simplify x =
    (parse x).Simplify()

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
    (parse expr).Solve(parseSymbol x)

/// Gets the LaTeX form of an expression
let latex x =
    (parse x).Latexise()

/// Substitutes the given variable with the given value in the given expression
let substitute x value expr = (parse expr).Substitute(parse x, parse value)

/// Returns a multiline string representation of matrix
let matrixToString (x: AngouriMath.Entity.Matrix) = x.ToString(true)

/// Returns a matrix modified according to the modifier
let modifiedMatrix (x: AngouriMath.Entity.Matrix) m =
    x.With(new System.Func<int, int, AngouriMath.Entity, AngouriMath.Entity>(m))

/// Gets the transposed form of a matrix or vector
let transposed (m: AngouriMath.Entity.Matrix) = m.T


/// Returns a conjunction node of two nodes
let conj a b = MathS.Conjunction(parse a, parse b)

/// Returns a disjunction node of two nodes
let disj a b = MathS.Disjunction(parse a, parse b)

/// Returns a implication node of two nodes
/// Where the first one is the assumption,
/// and the second one is the conclusion
let impl assum concl = MathS.Implication(parse assum, parse concl)

/// Returns a negation node of a node
let neg a = MathS.Negation(parse a)

/// Returns an exclusive disjunction node of two nodes
let xor a b = MathS.ExclusiveDisjunction(parse a, parse b)

/// Returns a union node of two nodes
let union a b = MathS.Union(parse a, parse b)

/// Returns a intersection node of two nodes
let intersect a b = MathS.Intersection(parse a, parse b)

/// Returns a set subtraction node of two nodes
let setSubtraction a b = MathS.SetSubtraction(parse a, parse b)

/// Returns an equality node of two nodes (not an equation!)
let equal a b = MathS.Equality(parse a, parse b)

/// Returns a greater than node of two nodes (a > b)
let greater a b = MathS.GreaterThan(parse a, parse b)

/// Returns a less than node of two nodes (a < b)
let less a b = MathS.LessThan(parse a, parse b)

/// Returns a greater or equal than node of two nodes (a >= b)
let greaterOrEqual a b = MathS.GreaterOrEqualThan(parse a, parse b)

/// Returns a less or equal than node of two nodes (a >= b)
let lessOrEqual a b = MathS.LessOrEqualThan(parse a, parse b)




/// Returns a logarithm node with the given base (as the first argument)
let log logBase x = MathS.Log(parse logBase, parse x)

/// Returns a logarithm node with e as the base
let ln x = MathS.Ln(parse x)

/// Returns a power of the expression with exponent equals 2
let sqr x = MathS.Sqr(parse x)

/// Returns a power of the expression with exponent equals 1 / 2
let sqrt x = MathS.Sqrt(parse x)

/// Returns a power of the expression with exponent equals 1 / 3
let cbrt x = MathS.Cbrt(parse x)

/// Returns a power of the expression with exponent as the second argument
let pow x power = MathS.Pow(parse x, parse power)

/// Returns a factorial node
let factorial x = MathS.Factorial(parse x)

/// Returns a gamma node
let gamma x = MathS.Gamma(parse x)

/// Returns a sign node
let sgn x = MathS.Signum(parse x)

/// Returns a sine node
let sin x = (parse x).Sin()

/// Returns a cosine node
let cos x = (parse x).Cos()

/// Returns a tangent node
let tan x = (parse x).Tan()

/// Returns a cotangent node
let cot x = (parse x).Cotan()

/// Returns a secant node
let sec x = (parse x).Sec()

/// Returns a cosecant node
let csc x = (parse x).Cosec()

/// Returns a arcsine node
let asin x = (parse x).Arcsin()

/// Returns a arccosine node
let acos x = (parse x).Arccos()

/// Returns a arctangent node
let atan x = (parse x).Arctan()

/// Returns a arccotangent node
let acot x = (parse x).Arccotan()

/// Returns a arcsecant node
let asec x = (parse x).Arcsec()

/// Returns a arccosecant node
let acsc x = (parse x).Arccosec()

/// Returns a hyperbolic sine node
let sinh x = MathS.Hyperbolic.Sinh(parse x)

/// Returns a hyperbolic cosine node
let cosh x = MathS.Hyperbolic.Cosh(parse x)

/// Returns a hyperbolic tangent node
let tanh x = MathS.Hyperbolic.Tanh(parse x)

/// Returns a hyperbolic cotangent node
let coth x = MathS.Hyperbolic.Cotanh(parse x)

/// Returns a hyperbolic secant node
let sech x = MathS.Hyperbolic.Sech(parse x)

/// Returns a hyperbolic cosecant node
let csch x = MathS.Hyperbolic.Cosech(parse x)

/// Returns a hyperbolic arsine node
let asinh x = MathS.Hyperbolic.Arsinh(parse x)

/// Returns a hyperbolic arcosine node
let acosh x = MathS.Hyperbolic.Arcosh(parse x)

/// Returns a hyperbolic artangent node
let atanh x = MathS.Hyperbolic.Artanh(parse x)

/// Returns a hyperbolic arcotangent node
let acoth x = MathS.Hyperbolic.Arcotanh(parse x)

/// Returns a hyperbolic arsecant node
let asech x = MathS.Hyperbolic.Arsech(parse x)

/// Returns a hyperbolic arcosecant node
let acsch x = MathS.Hyperbolic.Arcosech(parse x)

/// Computes the derivative of the given variable and expression
let differentiate x expr =
    (parse expr).Differentiate(parseSymbol x)

/// Returns a derivative node
let derivative x expr =
    MathS.Derivative(parse expr, parse x)

/// Computes the integral of the given variable and expression
let integrate x expr = 
    (parse expr).Integrate(parseSymbol x)

/// Returns an integral node
let integral x expr =
    MathS.Integral(parse expr, parse x)

/// Computes the limit of the given variable, destination to where it approaches, and expression
let limit x destination expr =
    (parse expr).Limit(parseSymbol x, parse destination)

/// Returns a limit node
let limited x destination expr =
    MathS.Limit(parse expr, parse x, parse destination)

/// Computes the limit of the given variable, destination to where it approaches, expression itself, and the origin side
let limitSided x destination side expr =
    match side with
    | Left -> (parse expr).Limit(parseSymbol x, parse destination, AngouriMath.Core.ApproachFrom.Left)
    | Right -> (parse expr).Limit(parseSymbol x, parse destination, AngouriMath.Core.ApproachFrom.Right)

/// Returns a limit node with a side from which to approach specified
let limitedSided x destination side expr =
    match side with
    | Left -> MathS.Limit(parse expr, parse x, parse destination, AngouriMath.Core.ApproachFrom.Left)
    | Right -> MathS.Limit(parse expr, parse x, parse destination, AngouriMath.Core.ApproachFrom.Right)