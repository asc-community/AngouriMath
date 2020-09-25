
/* Copyright (c) 2019-2020 Angourisoft
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
 * is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using AngouriMath.Core;
using NumericsComplex = System.Numerics.Complex;
using PeterO.Numbers;
using System.Linq;
using System.Collections.Generic;
using AngouriMath.Core.Exceptions;

namespace AngouriMath.Extensions
{
    using static Entity;
    using static Entity.Number;

    public static class AngouriMathExtensions
    {
        /// <summary>
        /// Converts your <see cref="IEnumerable"/> into a set of unique values.
        /// </summary>
        /// <returns>A Set</returns>
        public static Set ToSet(this IEnumerable<Entity> elements)
            => new Set(elements.Select(c => SetPiece.Element(c)).ToArray());

        /// <summary>
        /// Converts your <see cref="IEnumerable"/> into a set of unique values.
        /// </summary>
        /// <returns>A Set</returns>
        public static SetNode ToSetNode(this IEnumerable<Entity> elements)
            => new Set(elements.Select(c => SetPiece.Element(c)).ToArray());

        /// <summary>
        /// Converts your <see cref="IEnumerable"/> into a set of unique values.
        /// Guarantees the finiteness of the set
        /// </summary>
        /// <returns>A finite set of elements</returns>
        public static FiniteSet ToFiniteSet(this IEnumerable<Entity> expr) 
            => new FiniteSet(expr.Select(c => SetPiece.Element(c)));

        /// <summary>
        /// Unites your <see cref="IEnumerable"/> into one <see cref="SetNode"/>.
        /// Applies the "or" operator on those nodes
        /// </summary>
        /// <returns>A set of unique elements</returns>
        public static SetNode Unite(this IEnumerable<SetNode> sets)
            => sets.Aggregate((a, b) => a | b);

        /// <summary>
        /// Computes the intersection of your <see cref="IEnumerable"/>'s and makes it one <see cref="SetNode"/>.
        /// Applies the "and" operator on those nodes
        /// </summary>
        /// <returns>A set of unique elements</returns>
        public static SetNode Intersect(this IEnumerable<SetNode> sets)
            => sets.Aggregate((a, b) => a & b);

        /// <summary>
        /// Parses the expression into <see cref="Entity"/>.
        /// Synonymical to <see cref="MathS.FromString(string)"/>
        /// </summary>
        /// <returns>Expression</returns>
        public static Entity ToEntity(this string expr) => MathS.FromString(expr);

        /// <summary>
        /// Parses this and simplifies by running <see cref="Entity.Simplify()"/>
        /// </summary>
        /// <returns>Simplified expression</returns>
        public static Entity Simplify(this string expr) => expr.ToEntity().Simplify();

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
        /// <param name="var">A variable to substitute</param>
        /// <param name="value">A value to substitute <paramref name="var"/></param>
        /// <returns>Expression with substituted the variable</returns>
        public static Entity Substitute(this string expr, Variable var, Entity value)
            => expr.ToEntity().Substitute(var, value);

        /// <summary>
        /// Solves the given equation
        /// </summary>
        /// <param name="x">The variable to solve over</param>
        /// <returns>A <see cref="SetNode"/> of roots</returns>
        public static SetNode SolveEquation(this string expr, Variable x)
            => expr.ToEntity().SolveEquation(x);

        /// <summary>
        /// Solves the statement. The given expression must be boolean type,
        /// for example, equality, or boolean operators.
        /// </summary>
        /// <param name="vars">The variables over which to solve</param>
        /// <returns>A <see cref="SetNode"/> of roots</returns>
        public static SetNode Solve(this string expr, params Variable[] vars)
            => expr.ToEntity().Solve(vars);

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
        /// <param name="variables">The array of variables should cover all variables from the expression</param>
        /// <returns>A compiled expression</returns>
        public static FastExpression Compile(this string str, params Variable[] variables)
            => str.ToEntity().Compile(variables);

        /// <summary>
        /// Finds out the derivative of the given expression 
        /// </summary>
        /// <param name="x">A Variable over which to find a derivative</param>
        /// <returns>A derived expression</returns>
        public static Entity Derive(this string str, Variable x)
            => str.ToEntity().Derive(x);

        /*

            Utils/generate_tuples.bat to regenerate this block

        */

        ///<summary>Solves a given set of arbitrary equations</summary>
        ///<returns>A tensor whose width is 2 columns long or null if no solutions were found</returns>
        public static Tensor? SolveSystem(this (string eq1, string eq2) eqs, string var1, string var2)
            => MathS.Equations(eqs.eq1, eqs.eq2).Solve(var1, var2);

        ///<summary>Solves a given set of arbitrary equations</summary>
        ///<returns>A tensor whose width is 3 columns long or null if no solutions were found</returns>
        public static Tensor? SolveSystem(this (string eq1, string eq2, string eq3) eqs, string var1, string var2, string var3)
            => MathS.Equations(eqs.eq1, eqs.eq2, eqs.eq3).Solve(var1, var2, var3);

        ///<summary>Solves a given set of arbitrary equations</summary>
        ///<returns>A tensor whose width is 4 columns long or null if no solutions were found</returns>
        public static Tensor? SolveSystem(this (string eq1, string eq2, string eq3, string eq4) eqs, string var1, string var2, string var3, string var4)
            => MathS.Equations(eqs.eq1, eqs.eq2, eqs.eq3, eqs.eq4).Solve(var1, var2, var3, var4);

        ///<summary>Solves a given set of arbitrary equations</summary>
        ///<returns>A tensor whose width is 5 columns long or null if no solutions were found</returns>
        public static Tensor? SolveSystem(this (string eq1, string eq2, string eq3, string eq4, string eq5) eqs, string var1, string var2, string var3, string var4, string var5)
            => MathS.Equations(eqs.eq1, eqs.eq2, eqs.eq3, eqs.eq4, eqs.eq5).Solve(var1, var2, var3, var4, var5);

        ///<summary>Solves a given set of arbitrary equations</summary>
        ///<returns>A tensor whose width is 6 columns long or null if no solutions were found</returns>
        public static Tensor? SolveSystem(this (string eq1, string eq2, string eq3, string eq4, string eq5, string eq6) eqs, string var1, string var2, string var3, string var4, string var5, string var6)
            => MathS.Equations(eqs.eq1, eqs.eq2, eqs.eq3, eqs.eq4, eqs.eq5, eqs.eq6).Solve(var1, var2, var3, var4, var5, var6);

        ///<summary>Solves a given set of arbitrary equations</summary>
        ///<returns>A tensor whose width is 7 columns long or null if no solutions were found</returns>
        public static Tensor? SolveSystem(this (string eq1, string eq2, string eq3, string eq4, string eq5, string eq6, string eq7) eqs, string var1, string var2, string var3, string var4, string var5, string var6, string var7)
            => MathS.Equations(eqs.eq1, eqs.eq2, eqs.eq3, eqs.eq4, eqs.eq5, eqs.eq6, eqs.eq7).Solve(var1, var2, var3, var4, var5, var6, var7);

        ///<summary>Solves a given set of arbitrary equations</summary>
        ///<returns>A tensor whose width is 8 columns long or null if no solutions were found</returns>
        public static Tensor? SolveSystem(this (string eq1, string eq2, string eq3, string eq4, string eq5, string eq6, string eq7, string eq8) eqs, string var1, string var2, string var3, string var4, string var5, string var6, string var7, string var8)
            => MathS.Equations(eqs.eq1, eqs.eq2, eqs.eq3, eqs.eq4, eqs.eq5, eqs.eq6, eqs.eq7, eqs.eq8).Solve(var1, var2, var3, var4, var5, var6, var7, var8);

        ///<summary>Solves a given set of arbitrary equations</summary>
        ///<returns>A tensor whose width is 9 columns long or null if no solutions were found</returns>
        public static Tensor? SolveSystem(this (string eq1, string eq2, string eq3, string eq4, string eq5, string eq6, string eq7, string eq8, string eq9) eqs, string var1, string var2, string var3, string var4, string var5, string var6, string var7, string var8, string var9)
            => MathS.Equations(eqs.eq1, eqs.eq2, eqs.eq3, eqs.eq4, eqs.eq5, eqs.eq6, eqs.eq7, eqs.eq8, eqs.eq9).Solve(var1, var2, var3, var4, var5, var6, var7, var8, var9);
    }
}