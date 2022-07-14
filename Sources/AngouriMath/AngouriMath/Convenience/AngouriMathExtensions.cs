//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using NumericsComplex = System.Numerics.Complex;
using PeterO.Numbers;
using AngouriMath.Core.Exceptions;
using System;
using System.Collections;

namespace AngouriMath.Extensions
{
    using static AngouriMath.Entity.Set;
    using static Entity;
    using static Entity.Number;

    /// <summary>
    /// Class for some convenient extensions
    /// </summary>
    public static partial class AngouriMathExtensions
    {
        /// <summary>
        /// Concatenates the argument matrix to the right of the current.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Matrix ConcatToTheRight(this Matrix a, Matrix b)
            => MathS.Matrices.Concat(MathS.Matrices.Direction.Horizontal, a, b);
        
        /// <summary>
        /// Concatenates the argument matrix to the bottom of the current.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Matrix ConcatToTheBottom(this Matrix a, Matrix b)
            => MathS.Matrices.Concat(MathS.Matrices.Direction.Vertical, a, b);
    
        /// <summary>
        /// Converts a given sequence of elements into a vector,
        /// which is a one-column matrix
        /// </summary>
        public static Matrix ToVector(this IEnumerable<Entity> elements)
            => MathS.Vector(elements.ToArray());

        /// <summary>
        /// Sums all the given terms and returns the resulting expression
        /// new Entity[]{ 1, 2, 3 }.SumAll() -> "1 + 2 + 3"
        /// </summary>
        public static Entity SumAll(this IEnumerable<Entity> terms)
            => TreeAnalyzer.MultiHangBinary(terms.ToArray(), (a, b) => a + b);

        /// <summary>
        /// Multiplies all the given terms and returns the resulting expression
        /// new Entity[]{ 1, 2, 3 }.MultiplyAll() -> "1 * 2 * 3"
        /// </summary>
        public static Entity MultiplyAll(this IEnumerable<Entity> terms)
            => TreeAnalyzer.MultiHangBinary(terms.ToArray(), (a, b) => a * b);

        /// <summary>
        /// Converts an <see cref="IEnumerable"/> into a piecewise function
        /// </summary>
        /// <returns>A Piecewise node</returns>
        public static Piecewise ToPiecewise(this IEnumerable<Providedf> cases)
            => new Piecewise(cases);

        /// <summary>
        /// Converts a tuple of an expression and its predicate to a 
        /// Provided node
        /// </summary>
        /// <returns>Providedf node</returns>
        public static Providedf ToProvided(this (Entity expr, Entity pred) @this)
            => new Providedf(@this.expr, @this.pred);

        /// <summary>
        /// Converts your <see cref="IEnumerable"/> into a set of unique values.
        /// </summary>
        /// <returns>A Set</returns>
        public static FiniteSet ToSet(this IEnumerable<Entity> elements)
            => new FiniteSet(elements);

        /// <summary>
        /// Unites your <see cref="IEnumerable"/> into one <see cref="Set"/>.
        /// Applies the "or" operator on those nodes
        /// </summary>
        /// <returns>A set of unique elements</returns>
        public static Set Unite(this IEnumerable<Set> sets)
            => sets.Any() ? sets.Aggregate((a, b) => MathS.Union(a, b)) : Empty;

        /// <summary>
        /// Computes the intersection of your <see cref="IEnumerable"/>'s and makes it one <see cref="Set"/>.
        /// Applies the "and" operator on those nodes
        /// </summary>
        /// <returns>A set of unique elements</returns>
        public static Set Intersect(this IEnumerable<Set> sets)
            => sets.Any() ? sets.Aggregate((a, b) => MathS.Intersection(a, b)) : Empty;

        /// <summary>
        /// Parses the expression into <see cref="Entity"/>.
        /// Synonymical to <see cref="MathS.FromString(string)"/>
        /// </summary>
        /// <returns>Expression</returns>
        public static Entity ToEntity(this string expr) => MathS.FromString(expr);

        /// <summary>
        /// Takes a tuple of four and builds an interval
        /// </summary>
        public static Interval ToEntity(this (Entity left, bool leftClosed, Entity right, bool rightClosed) arg)
            => new Interval(arg.left, arg.leftClosed, arg.right, arg.rightClosed);

        /// <summary>
        /// Parses this and simplifies by running <see cref="Entity.Simplify"/>
        /// </summary>
        /// <returns>Simplified expression</returns>
        public static Entity Simplify(this string expr) => expr.ToEntity().Simplify();

        /// <summary>
        /// Parses this and simplifies by running <see cref="Entity.Simplify"/>
        /// </summary>
        /// <returns>Simplified expression</returns>
        public static Entity Simplify(this string expr, int level) => expr.ToEntity().Simplify(level);

        /// <summary>
        /// Parses this and evals into a number by running <see cref="Entity.EvalNumerical"/>
        /// </summary>
        /// <exception cref="CannotEvalException">
        /// This thrown when the given expression is boolean, tensoric, or contains variables.
        /// First, check whether it can be evaled: <see cref="Entity.EvaluableNumerical"/>
        /// </exception>
        /// <returns>Collapses into one expression</returns>
        public static Complex EvalNumerical(this string expr) => expr.ToEntity().EvalNumerical();

        /// <summary>
        /// Parses this and evals into a boolean by running <see cref="Entity.EvalBoolean"/>
        /// </summary>
        /// <exception cref="CannotEvalException">
        /// This thrown when the given expression is numerical, tensoric, or contains variables.
        /// First, check whether it can be evaled: <see cref="Entity.EvaluableBoolean"/>
        /// </exception>
        /// <returns>Collapses into one expression</returns>
        public static Boolean EvalBoolean(this string expr) => expr.ToEntity().EvalBoolean();

        /// <summary>
        /// Parses and expands the given expression so that as many parentheses as possible
        /// get expanded into a linear expression.
        /// </summary>
        /// <returns>An expanded expression</returns>
        public static Entity Expand(this string expr) => expr.ToEntity().Expand();

        /// <summary>
        /// Parses and factorizes the given expression so that as few powers as possible remain,
        /// and the expression is represented as a product of multipliers
        /// </summary>
        /// <returns>A factorized expression</returns>
        public static Entity Factorize(this string expr) => expr.ToEntity().Factorize();

        /// <summary>
        /// Subsitutes a variable by replacing all its occurances with the given value
        /// </summary>
        /// <param name="expr">The expression where to substitute the variables</param>
        /// <param name="var">A variable to substitute</param>
        /// <param name="value">A value to substitute <paramref name="var"/></param>
        /// <returns>Expression with substituted the variable</returns>
        public static Entity Substitute(this string expr, Variable var, Entity value)
            => expr.ToEntity().Substitute(var, value);

        /// <summary>
        /// Replaces x.x1 with value.v1 and
        /// x.x2 with value.v2
        /// </summary>
        public static Entity Substitute(this string expr, (Entity x1, Entity x2) x, (Entity v1, Entity v2) value)
            => expr.ToEntity().Substitute(x.x1, value.v1).Substitute(x.x2, value.v2);

        /// <summary>
        /// Replaces x.x1 with value.v1 and
        /// x.x2 with value.v2 and
        /// x.x3 with value.v3
        /// </summary>
        public static Entity Substitute(this string expr, (Entity x1, Entity x2, Entity x3) x, (Entity v1, Entity v2, Entity v3) value)
            => expr.ToEntity().Substitute(x.x1, value.v1).Substitute(x.x2, value.v2).Substitute(x.x3, value.v3);

        /// <summary>
        /// Replaces x.x1 with value.v1 and
        /// x.x2 with value.v2 and
        /// x.x3 with value.v3 and
        /// x.x4 with value.v4
        /// </summary>
        public static Entity Substitute(this string expr, (Entity x1, Entity x2, Entity x3, Entity x4) x, (Entity v1, Entity v2, Entity v3, Entity v4) value)
            => expr.ToEntity().Substitute(x.x1, value.v1).Substitute(x.x2, value.v2).Substitute(x.x3, value.v3).Substitute(x.x4, value.v4);

        /// <summary>
        /// Solves the given equation
        /// </summary>
        /// <param name="expr">The function of <paramref name="x"/> that is assumed to be 0</param>
        /// <param name="x">The variable to solve over</param>
        /// <returns>A <see cref="Set"/> of roots</returns>
        public static Set SolveEquation(this string expr, Variable x)
            => expr.ToEntity().SolveEquation(x);

        /// <summary>
        /// Solves the statement. The given expression must be boolean type,
        /// for example, equality, or boolean operators.
        /// </summary>
        /// <param name="expr">The statement of <paramref name="var"/> that is assumed to be true</param>
        /// <param name="var">The variables over which to solve</param>
        /// <returns>A <see cref="Set"/> of roots</returns>
        public static Set Solve(this string expr, Variable var)
            => expr.ToEntity().Solve(var);

        /// <summary>
        /// Converts an <see cref="int"/> into an AM's understandable <see cref="Integer"/>
        /// which can be hung with others
        /// </summary>
        /// <returns>AM's <see cref="Integer"/></returns>
        public static Integer ToNumber(this int value) => Integer.Create(value);

        /// <summary>
        /// Converts an <see cref="long"/> into an AM's understandable <see cref="Integer"/>
        /// which can be hung with others
        /// </summary>
        /// <returns>AM's <see cref="Integer"/></returns>
        public static Integer ToNumber(this long value) => Integer.Create(value);

        /// <summary>
        /// Converts PeterO's <see cref="EInteger"/> into an AM's understandable <see cref="Integer"/>
        /// which can be hung with others
        /// </summary>
        /// <returns>AM's <see cref="Integer"/></returns>
        public static Integer ToNumber(this EInteger value) => Integer.Create(value);

        /// <summary>
        /// Converts an <see cref="float"/> into an AM's understandable <see cref="Real"/>
        /// which can be hung with others
        /// </summary>
        /// <returns>AM's <see cref="Real"/></returns>
        public static Real ToNumber(this float value) => Real.Create(EDecimal.FromSingle(value));

        /// <summary>
        /// Converts an <see cref="double"/> into an AM's understandable <see cref="Real"/>
        /// which can be hung with others
        /// </summary>
        /// <returns>AM's <see cref="Real"/></returns>
        public static Real ToNumber(this double value) => Real.Create(EDecimal.FromDouble(value));

        /// <summary>
        /// Converts an <see cref="decimal"/> into an AM's understandable <see cref="Real"/>
        /// which can be hung with others
        /// </summary>
        /// <returns>AM's <see cref="Real"/></returns>
        public static Real ToNumber(this decimal value) => Real.Create(EDecimal.FromDecimal(value));

        /// <summary>
        /// Converts PeterO's <see cref="EDecimal"/> into an AM's understandable <see cref="Real"/>
        /// which can be hung with others
        /// </summary>
        /// <returns>AM's <see cref="Real"/></returns>
        public static Real ToNumber(this EDecimal value) => Real.Create(value);

        /// <summary>
        /// Converts Numerics's <see cref="NumericsComplex"/> into an AM's understandable <see cref="Complex"/>
        /// which can be hung with others
        /// </summary>
        /// <returns>AM's <see cref="Complex"/></returns>
        public static Complex ToNumber(this NumericsComplex complex)
            => Complex.Create(EDecimal.FromDouble(complex.Real), EDecimal.FromDouble(complex.Imaginary));

        /// <summary>
        /// Converts an <see cref="bool"/> into an AM's understandable <see cref="Boolean"/>
        /// which can be hung with others
        /// </summary>
        /// <returns>AM's <see cref="Boolean"/></returns>
        public static Boolean ToBoolean(this bool value) => Boolean.Create(value);
        
        /// <summary>
        /// Builds a LaTeX code from an expression
        /// </summary>
        /// <returns>A <see cref="string"/> which can be rendered into pretty output</returns>
        public static string Latexise(this string str) => str.ToEntity().Latexise();

        /// <summary>
        /// Compiles an expression into a special compiled code that runs via
        /// AM's virtual machine. Soon will be deprecated and replaced with compilation to
        /// delegate
        /// </summary>
        /// <param name="str">From which function to compile</param>
        /// <param name="variables">The array of variables should cover all variables from the expression</param>
        /// <returns>A compiled expression</returns>
        public static FastExpression Compile(this string str, params Variable[] variables)
            => str.ToEntity().Compile(variables);



        /// <summary>
        /// Finds the symbolical derivative of the given expression
        /// </summary>
        /// <param name="str">
        /// The expresion to be parsed and differentiated
        /// </param>
        /// <param name="x">
        /// Over which variable to find the derivative
        /// </param>
        /// <returns>
        /// The derived expression which might contain <see cref="Derivativef"/> nodes,
        /// or the initial one
        /// </returns>
        [Obsolete("Renamed to Differentiate")]
        public static Entity Derive(this string str, Variable x)
            => str.ToEntity().Differentiate(x);

        /// <summary>
        /// Finds the symbolical derivative of the given expression
        /// </summary>
        /// <param name="str">
        /// The expresion to be parsed and differentiated
        /// </param>
        /// <param name="x">
        /// Over which variable to find the derivative
        /// </param>
        /// <returns>
        /// The derived expression which might contain <see cref="Derivativef"/> nodes,
        /// or the initial one
        /// </returns>
        public static Entity Differentiate(this string str, Variable x)
            => str.ToEntity().Differentiate(x);

        /// <summary>
        /// Integrates the given expression over the `x` variable, if can.
        /// May return an unresolved <see cref="Integralf"/> node.
        /// </summary>
        /// <param name="str">
        /// The expression to be parsed and integrated over <paramref name="x"/>
        /// </param>
        /// <param name="x">Over which to integrate</param>
        /// <returns>
        /// An integrated expression. It might remain the same,
        /// it might have no integrals, and it might be transformed so that
        /// only a few nodes have unresolved integrals.
        /// </returns>
        public static Entity Integrate(this string str, Variable x)
            => str.ToEntity().Integrate(x);

        /// <summary>
        /// Finds the limit of the given expression over the given variable
        /// </summary>
        /// <param name="str">
        /// The expression to be parsed and whose limit to be computed
        /// </param>
        /// <param name="x">
        /// The variable to be approaching
        /// </param>
        /// <param name="destination">
        /// A value where the variable approaches. It might be a symbolic
        /// expression, a finite number, or an infinite number, for example,
        /// "sqrt(x2 + x) / (3x + 3)".Limit("x", "+oo", ApproachFrom.BothSides)
        /// </param>
        /// <param name="side">
        /// From where to approach it: from the left, from the right,
        /// or BothSides, implying that if limits from either are not
        /// equal, there is no limit
        /// </param>
        /// <returns>
        /// A result or the <see cref="Limitf"/> node if the limit
        /// cannot be determined
        /// </returns>
        public static Entity Limit(this string str, Variable x, Entity destination, ApproachFrom side)
            => str.ToEntity().Limit(x, destination, side);

        /// <summary>
        /// Finds the limit of the given expression over the given variable
        /// </summary>
        /// <param name="str">The expression to be parsed and whose limit to be found</param>
        /// <param name="x">
        /// The variable to be approaching
        /// </param>
        /// <param name="destination">
        /// A value where the variable approaches. It might be a symbolic
        /// expression, a finite number, or an infinite number, for example,
        /// "sqrt(x2 + x) / (3x + 3)".Limit("x", "+oo")
        /// </param>
        /// <returns>
        /// A result or the <see cref="Limitf"/> node if the limit
        /// cannot be determined
        /// </returns>
        public static Entity Limit(this string str, Variable x, Entity destination)
            => str.ToEntity().Limit(x, destination);
    }
}