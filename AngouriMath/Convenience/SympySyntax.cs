
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

using System.Linq;
using AngouriMath.Core;

namespace AngouriMath
{
    using static Entity;
    /// <summary>
    /// Sympy - is a super-powerful library for algebra cumputation,
    /// multi-variable expressions, physics, matrices, etc.
    /// But it is only available for python. If you need the same syntax for C#,
    /// this module is for you.
    /// </summary>
    public static class SySyn
    {
        /// Here sympy-like syntax implementation goes
        /// <summary>
        /// Differentiation
        /// </summary>
        /// <param name="expr">
        /// Expression to differentiate
        /// </param>
        /// <param name="vars">
        /// Variable to differentiate over. If you need more than first derivative,
        /// specify as many variables as you need
        /// </param>
        /// <returns></returns>
        public static Entity Diff(Entity expr, params Variable[] vars)
        {
            expr = vars.Aggregate(expr, (current, v) => current.Derive(v));
            return expr.Simplify();
        }

        /// <summary>
        /// Simplification of expression
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static Entity Simplify(Entity expr) => expr.Simplify();

        /// <summary>
        /// Attempt to find analytical roots of a custom equation
        /// </summary>
        /// <param name="x"></param>
        /// <returns>
        /// Returns Set. Work with it as with a list
        /// </returns>
        public static SetNode Solve(Entity expr, Variable x) => expr.SolveEquation(x);

        /// <summary>
        /// Expands an equation trying to eliminate all the parentheses ( e. g. 2 * (x + 3) = 2 * x + 2 * 3 )
        /// </summary>
        /// <returns>
        /// An expanded Entity
        /// </returns>
        public static Entity Expand(Entity expr) => expr.Expand();

        /// <summary>
        /// Factorizes an equation trying to eliminate as many power-uses as possible ( e. g. x * 3 + x * y = x * (3 + y) )
        /// </summary>
        public static Entity Factor(Entity expr) => expr.Factorize();

        /// <summary>
        /// Simplification synonym. Recommended to use in case of 
        /// computing a concrete number, knowing that you don't have
        /// any other symbols but numbers and functions.
        /// </summary>
        public static Number.Complex Evalf(Entity expr) => expr.EvalNumerical();

        /// <returns>
        /// Returns the expression in format of latex (for example, a / b -> \frac{a}{b})
        /// </returns>
        public static Entity Latex(Entity expr) => expr.Latexise();

        /// <summary>Creates an instance of <see cref="Variable"/>.</summary>
        public static Variable Symbol(string name) => name;
        /// <summary>Creates two instances of <see cref="Variable"/>.</summary>
        public static (Variable, Variable) Symbol(string name1, string name2) => (name1, name2);
        /// <summary>Creates three instances of <see cref="Variable"/>.</summary>
        public static (Variable, Variable, Variable) Symbol(string name1, string name2, string name3) => (name1, name2, name3);
        /// <summary>Creates four instances of <see cref="Variable"/>.</summary>
        public static (Variable, Variable, Variable, Variable) Symbol(string name1, string name2, string name3, string name4) => (name1, name2, name3, name4);

        /// <summary>
        /// e ^ power
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        public static Entity Exp(Entity power) => MathS.Pow(MathS.e, power);

        /// <summary>
        /// Creates an instance of fraction
        /// </summary>
        /// <param name="a">
        /// Numerator
        /// </param>
        /// <param name="b">
        /// Denominator
        /// </param>
        /// <returns></returns>
        public static Entity Rational(Number.Complex a, Number.Complex b) => a / b;
    }
}