module AngouriMath.FSharp.Functions

open Core
open AngouriMath
open System.Linq

/// Creates a node of power and inner simplifies it
let ( ** ) a b =
    MathS.Pow(a, b).InnerSimplified

/// simplifies the given expression
let simplified x =
    (parsed x).Simplify()

/// Takes the evaluated form of an expression (preserving the type)
let evaled expr =
    (parsed expr).Evaled

/// Takes the evaluated form of an expression (as a number; if it cannot, an exception is thrown)
let asNumber expr =
    (parsed expr).EvalNumerical()

/// Takes the evaluated form of an expression (as a boolean; if it cannot, an exception is thrown)
let asBool expr =
    (parsed expr).EvalBoolean()

/// Gets the LaTeX form of an expression
let latex x =
    (parsed x).Latexise()

/// Substitutes the given variable with the given value in the given expression
let substituted x value expr = (parsed expr).Substitute(parsed x, parsed value)

/// Returns a multiline string representation of matrix
let printedMatrix (x: AngouriMath.Entity.Matrix) = x.ToString(true)

/// Returns a matrix modified according to the modifier
let modifiedMatrix (x: AngouriMath.Entity.Matrix) m =
    x.With(new System.Func<int, int, AngouriMath.Entity, AngouriMath.Entity>(m))

/// Gets the transposed form of a matrix or vector
let transposed (m: AngouriMath.Entity.Matrix) = m.T


/// Returns a conjunction node of two nodes
let conjunction a b = MathS.Conjunction(parsed a, parsed b)

/// Returns a disjunction node of two nodes
let disjunction a b = MathS.Disjunction(parsed a, parsed b)

/// Returns a implication node of two nodes
/// Where the first one is the assumption,
/// and the second one is the conclusion
let implication assum concl = MathS.Implication(parsed assum, parsed concl)

/// Returns a negation node of a node
let negation a = MathS.Negation(parsed a)

/// Returns an exclusive disjunction node of two nodes
let exDisjunction a b = MathS.ExclusiveDisjunction(parsed a, parsed b)

/// Returns a union node of two nodes
let union a b = MathS.Union(parsed a, parsed b)

/// Returns a intersection node of two nodes
let intersect a b = MathS.Intersection(parsed a, parsed b)

/// Returns a set subtraction node of two nodes
let setSubtraction a b = MathS.SetSubtraction(parsed a, parsed b)

/// Returns an equality node of two nodes (not an equation!)
let equality a b = MathS.Equality(parsed a, parsed b)

/// Returns a greater than node of two nodes (a > b)
let greater a b = MathS.GreaterThan(parsed a, parsed b)

/// Returns a less than node of two nodes (a < b)
let less a b = MathS.LessThan(parsed a, parsed b)

/// Returns a greater or equal than node of two nodes (a >= b)
let greaterOrEqual a b = MathS.GreaterOrEqualThan(parsed a, parsed b)

/// Returns a less or equal than node of two nodes (a >= b)
let lessOrEqual a b = MathS.LessOrEqualThan(parsed a, parsed b)




/// Returns a logarithm node with the given base (as the first argument)
let log logBase x = MathS.Log(parsed logBase, parsed x)

/// Returns a logarithm node with e as the base
let ln x = MathS.Ln(parsed x)

/// Returns a power of the expression with exponent equals 2
let sqr x = MathS.Sqr(parsed x)

/// Returns a power of the expression with exponent equals 1 / 2
let sqrt x = MathS.Sqrt(parsed x)

/// Returns a power of the expression with exponent equals 1 / 3
let cbrt x = MathS.Cbrt(parsed x)

/// Returns a power of the expression with exponent as the second argument
let pow x power = MathS.Pow(parsed x, parsed power)

/// Returns a factorial node
let factorial x = MathS.Factorial(parsed x)

/// Returns a gamma node
let gamma x = MathS.Gamma(parsed x)

/// Returns a sign node
let sgn x = MathS.Signum(parsed x)

/// Returns a sine node
let sin x = (parsed x).Sin()

/// Returns a cosine node
let cos x = (parsed x).Cos()

/// Returns a tangent node
let tan x = (parsed x).Tan()

/// Returns a cotangent node
let cot x = (parsed x).Cotan()

/// Returns a secant node
let sec x = (parsed x).Sec()

/// Returns a cosecant node
let csc x = (parsed x).Cosec()

/// Returns a arcsine node
let asin x = (parsed x).Arcsin()

/// Returns a arccosine node
let acos x = (parsed x).Arccos()

/// Returns a arctangent node
let atan x = (parsed x).Arctan()

/// Returns a arccotangent node
let acot x = (parsed x).Arccotan()

/// Returns a arcsecant node
let asec x = (parsed x).Arcsec()

/// Returns a arccosecant node
let acsc x = (parsed x).Arccosec()

/// Returns a hyperbolic sine node
let sinh x = MathS.Hyperbolic.Sinh(parsed x)

/// Returns a hyperbolic cosine node
let cosh x = MathS.Hyperbolic.Cosh(parsed x)

/// Returns a hyperbolic tangent node
let tanh x = MathS.Hyperbolic.Tanh(parsed x)

/// Returns a hyperbolic cotangent node
let coth x = MathS.Hyperbolic.Cotanh(parsed x)

/// Returns a hyperbolic secant node
let sech x = MathS.Hyperbolic.Sech(parsed x)

/// Returns a hyperbolic cosecant node
let csch x = MathS.Hyperbolic.Cosech(parsed x)

/// Returns a hyperbolic arsine node
let asinh x = MathS.Hyperbolic.Arsinh(parsed x)

/// Returns a hyperbolic arcosine node
let acosh x = MathS.Hyperbolic.Arcosh(parsed x)

/// Returns a hyperbolic artangent node
let atanh x = MathS.Hyperbolic.Artanh(parsed x)

/// Returns a hyperbolic arcotangent node
let acoth x = MathS.Hyperbolic.Arcotanh(parsed x)

/// Returns a hyperbolic arsecant node
let asech x = MathS.Hyperbolic.Arsech(parsed x)

/// Returns a hyperbolic arcosecant node
let acsch x = MathS.Hyperbolic.Arcosech(parsed x)



/// Returns a bounded closed interval
let closedInterval from ``to`` =
    MathS.Interval(parsed from, parsed ``to``)

/// Returns a bounded open interval
let openInterval from ``to`` =
    MathS.Interval(parsed from, false, parsed ``to``, false)

/// Returns a bounded interval with left point included and right point excluded
let leftInclusiveRightExclusive from ``to`` =
    MathS.Interval(parsed from, true, parsed ``to``, false)

/// Returns a bounded interval with right point included and left point excluded
let leftExclusiveRightInclusive from ``to`` =
    MathS.Interval(parsed from, false, parsed ``to``, true)

/// Returns a half-bounded with left point included
let leftInclusive from =
    MathS.Interval(parsed from, true, parsed "+oo", false)

/// Returns a half-bounded with left point excluded
let leftExclusive from =
    MathS.Interval(parsed from, false, parsed "+oo", false)

/// Returns a half-bounded with right point included
let rightInclusive ``to`` =
    MathS.Interval(parsed "-oo", false, parsed ``to``, true)

/// Returns a half-bounded with right point excluded
let rightExclusive ``to`` =
    MathS.Interval(parsed "-oo", false, parsed ``to``, false)


    


/// Computes the derivative of the given variable and expression
let derivative x expr =
    (parsed expr).Differentiate(symbol x)

/// Returns a derivative node
let derivativeNode x expr =
    MathS.Derivative(parsed expr, parsed x)

/// Computes the integral of the given variable and expression
let integral x expr = 
    (parsed expr).Integrate(symbol x)

/// Returns an integral node
let integralNode x expr =
    MathS.Integral(parsed expr, parsed x)

/// Computes the limit of the given variable, destination to where it approaches, and expression
let limit x destination expr =
    (parsed expr).Limit(symbol x, parsed destination)

/// Returns a limit node
let limitNode x destination expr =
    MathS.Limit(parsed expr, parsed x, parsed destination)

type LimSide =
    | Left
    | Right

/// Computes the limit of the given variable, destination to where it approaches, expression itself, and the origin side
let limitSided x destination side expr =
    match side with
    | Left -> (parsed expr).Limit(symbol x, parsed destination, AngouriMath.Core.ApproachFrom.Left)
    | Right -> (parsed expr).Limit(symbol x, parsed destination, AngouriMath.Core.ApproachFrom.Right)

/// Returns a limit node with a side from which to approach specified
let limitSidedNode x destination side expr =
    match side with
    | Left -> MathS.Limit(parsed expr, parsed x, parsed destination, AngouriMath.Core.ApproachFrom.Left)
    | Right -> MathS.Limit(parsed expr, parsed x, parsed destination, AngouriMath.Core.ApproachFrom.Right)

/// Returns the set of possible values for x
/// provided the statement or equality
let solutions x expr =
    match parsed expr with
    | :? Entity.Statement as statement -> statement.Solve(symbol x)
    | func -> (equality func 0).Solve(symbol x)




type Lengths =
    | Any
    | Invalid
    | Fixed of int
    
/// Creates a matrix from a list of lists
/// of objects (which are parsed into Entities)
let matrix x = 
    let rec columnCount (x : 'T list list) =
        match x with
        | [] -> Any
        | hd::tl ->
            match columnCount tl with
            | Any -> Fixed(hd.Length)
            | Invalid -> Invalid
            | Fixed len -> if len = hd.Length then Fixed(len) else Invalid
    
    let parseListOfLists li =
        [ for row in li do yield [ for el in row do yield (parsed el) ] ]
    
    match columnCount x with
    | Any | Invalid -> raise ParseException
    | Fixed _ -> MathS.Matrix(array2D (parseListOfLists x))
    
    
/// Creates a column vector from a 1-dimensional list
let vector li =
    MathS.Vector([ for el in li do yield parsed el ].ToArray())
    
