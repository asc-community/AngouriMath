/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using PeterO.Numbers;
using AngouriMath.Core;
using AngouriMath.Functions;
using AngouriMath.Functions.Algebra;
using AngouriMath.Functions.Algebra.NumericalSolving;
using AngouriMath.Functions.Boolean;
using AngouriMath.Core.Exceptions;
using System.Diagnostics.CodeAnalysis;

namespace AngouriMath.Core
{
    public enum ApproachFrom
    {
        BothSides,
        Left,
        Right,
    }
}
namespace AngouriMath
{
    using static Entity;
    using static Entity.Number;
    using NumericsComplex = System.Numerics.Complex;
    using GenTensor = GenericTensor.Core.GenTensor<Entity, Entity.Tensor.EntityTensorWrapperOperations>;
    using static Entity.Set;
    using static AngouriMath.MathS.Settings;

    /// <summary>Use functions from this class</summary>
    public static class MathS
    {
        /// <summary>Use it to solve equations</summary>
        /// <param name="equations">
        /// An array of <see cref="Entity"/> (or <see cref="string"/>s)
        /// the system consists of
        /// </param>
        /// <returns>An <see cref="EquationSystem"/> which can then be solved</returns>
        public static EquationSystem Equations(params Entity[] equations) => new EquationSystem(equations);

        /// <summary>Use it to solve equations</summary>
        /// <param name="equations">
        /// An array of <see cref="Entity"/> (or <see cref="string"/>s)
        /// the system consists of
        /// </param>
        /// <returns>An <see cref="EquationSystem"/> which can then be solved</returns>
        public static EquationSystem Equations(IEnumerable<Entity> equations) => new EquationSystem(equations);

        /// <summary>Solves one equation over one variable</summary>
        /// <param name="equation">An equation that is assumed to equal 0</param>
        /// <param name="var">Variable whose values we are looking for</param>
        /// <returns>A <see cref="Set"/> of possible values or intervals of values</returns>
        public static Set SolveEquation(Entity equation, Variable var) => EquationSolver.Solve(equation, var);

        /// <summary>
        /// Solves a boolean expression. That is, finds all values for
        /// <paramref name="variables"/> such that the expression turns into True when evaluated
        /// Uses a simple table of truth
        /// Use <see cref="Entity.SolveBoolean(Variable)"/> for smart solving
        /// </summary>
        public static Tensor? SolveBooleanTable(Entity expression, params Variable[] variables)
            => BooleanSolver.SolveTable(expression, variables);


        /// <summary><a href="https://en.wikipedia.org/wiki/Trigonometric_functions"/></summary>
        /// <param name="a">Argument node of sine</param>
        /// <returns>Sine node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Sin(Entity a) => new Sinf(a);

        /// <summary><a href="https://en.wikipedia.org/wiki/Trigonometric_functions"/></summary>
        /// <param name="a">Argument node of cosine</param>
        /// <returns>Cosine node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Cos(Entity a) => new Cosf(a);

        /// <summary><a href="https://en.wikipedia.org/wiki/Logarithm"/></summary>
        /// <param name="base">Base node of logarithm</param>
        /// <param name="x">Argument node of logarithm</param>
        /// <returns>Logarithm node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Log(Entity @base, Entity x) => new Logf(@base, x);

        /// <summary><a href="https://en.wikipedia.org/wiki/Logarithm"/></summary>
        /// <param name="x">Argument node of logarithm</param>
        /// <returns>Logarithm node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Log(Entity x) => new Logf(10, x);

        /// <summary><a href="https://en.wikipedia.org/wiki/Power_function"/></summary>
        /// <param name="base">Base node of power</param>
        /// <param name="power">Argument node of power</param>
        /// <returns>Power node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Pow(Entity @base, Entity power) => new Powf(@base, power);

        /// <summary>Special case of <a href="https://en.wikipedia.org/wiki/Power_function"/></summary>
        /// <param name="a">The argument of which square root will be taken</param>
        /// <returns>Power node with (1/2) as the power</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Sqrt(Entity a) => new Powf(a, Number.Rational.Create(1, 2));

        /// <summary>Special case of <a href="https://en.wikipedia.org/wiki/Power_function"/></summary>
        /// <param name="a">The argument of which cube root will be taken</param>
        /// <returns>Power node with (1/3) as the power</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Cbrt(Entity a) => new Powf(a, Number.Rational.Create(1, 3));

        /// <summary>Special case of <a href="https://en.wikipedia.org/wiki/Power_function"/></summary>
        /// <param name="a">Argument to be squared</param>
        /// <returns>Power node with 2 as the power</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Sqr(Entity a) => new Powf(a, 2);

        /// <summary><a href="https://en.wikipedia.org/wiki/Trigonometric_functions"/></summary>
        /// <param name="a">Argument node of which tangent will be taken</param>
        /// <returns>Tangent node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Tan(Entity a) => new Tanf(a);

        /// <summary><a href="https://en.wikipedia.org/wiki/Trigonometric_functions"/></summary>
        /// <param name="a">Argument node of which cotangent will be taken</param>
        /// <returns>Cotangent node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Cotan(Entity a) => new Cotanf(a);

        /// <summary><a href="https://en.wikipedia.org/wiki/Trigonometric_functions"/></summary>
        /// <param name="a">Argument node of which secant will be taken</param>
        /// <returns>Reciprocal of cosine node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Sec(Entity a) => 1 / Cos(a);

        /// <summary><a href="https://en.wikipedia.org/wiki/Trigonometric_functions"/></summary>
        /// <param name="a">Argument node of which cosecant will be taken</param>
        /// <returns>Reciprocal of sine node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Cosec(Entity a) => 1 / Sin(a);

        /// <summary><a href="https://en.wikipedia.org/wiki/Inverse_trigonometric_functions"/></summary>
        /// <param name="a">Argument node of which arcsine will be taken</param>
        /// <returns>Arcsine node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Arcsin(Entity a) => new Arcsinf(a);

        /// <summary><a href="https://en.wikipedia.org/wiki/Inverse_trigonometric_functions"/></summary>
        /// <param name="a">Argument node of which arccosine will be taken</param>
        /// <returns>Arccosine node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Arccos(Entity a) => new Arccosf(a);

        /// <summary><a href="https://en.wikipedia.org/wiki/Inverse_trigonometric_functions"/></summary>
        /// <param name="a">Argument node of which arctangent will be taken</param>
        /// <returns>Arctangent node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Arctan(Entity a) => new Arctanf(a);

        /// <summary><a href="https://en.wikipedia.org/wiki/Inverse_trigonometric_functions"/></summary>
        /// <param name="a">Argument node of which arccotangent will be taken</param>
        /// <returns>Arccotangent node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Arccotan(Entity a) => new Arccotanf(a);

        /// <summary><a href="https://en.wikipedia.org/wiki/Inverse_trigonometric_functions"/></summary>
        /// <param name="a">Argument node of which arcsecant will be taken</param>
        /// <returns>Arccosine node with the reciprocal of the argument</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Arcsec(Entity a) => new Arccosf(1 / a);

        /// <summary><a href="https://en.wikipedia.org/wiki/Inverse_trigonometric_functions"/></summary>
        /// <param name="a">Argument node of which arccosecant will be taken</param>
        /// <returns>Arcsine node with the reciprocal of the argument</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Arccosec(Entity a) => new Arcsinf(1 / a);

        /// <summary>
        /// Is a special case of logarithm where the base equals
        /// <a href="https://en.wikipedia.org/wiki/E_(mathematical_constant)">e</a>:
        /// <a href="https://en.wikipedia.org/wiki/Natural_logarithm"/>
        /// </summary>
        /// <param name="a">Argument node of which natural logarithm will be taken</param>
        /// <returns>Logarithm node with base equal to e</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Ln(Entity a) => new Logf(e, a);

        /// <summary><a href="https://en.wikipedia.org/wiki/Factorial"/></summary>
        /// <param name="a">Argument node of which factorial will be taken</param>
        /// <returns>Factorial node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Factorial(Entity a) => new Factorialf(a);

        /// <summary><a href="https://en.wikipedia.org/wiki/Gamma_function"/></summary>
        /// <param name="a">Argument node of which gamma function will be taken</param>
        /// <returns>Factorial node with one added to the argument</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Gamma(Entity a) => new Factorialf(a + 1);

        /// <summary>https://en.wikipedia.org/wiki/Sign_function</summary>
        /// <param name="a">Argument node of which Signum function will be taken</param>
        /// <returns>Signum node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Signum(Entity a) => new Signumf(a);

        /// <summary>https://en.wikipedia.org/wiki/Absolute_value</summary>
        /// <param name="a">Argument node of which Abs function will be taken</param>
        /// <returns>Abs node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Abs(Entity a) => new Absf(a);

        /// <summary>https://en.wikipedia.org/wiki/Negation</summary>
        /// <param name="a">Argument node of which Negation function will be taken</param>
        /// <returns>Not node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Negation(Entity a) => !a;

        /// <summary>https://en.wikipedia.org/wiki/Logical_disjunction</summary>
        /// <param name="a">The left argument node of which Disjunction function will be taken</param>
        /// <param name="b">The right argument node of which Disjunction function will be taken</param>
        /// <returns>Or node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Disjunction(Entity a, Entity b) => a | b;

        /// <summary>https://en.wikipedia.org/wiki/Logical_conjunction</summary>
        /// <param name="a">Left argument node of which Conjunction function will be taken</param>
        /// <param name="b">Right argument node of which Conjunction disjunction function will be taken</param>
        /// <returns>And node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Conjunction(Entity a, Entity b) => a & b;

        /// <summary>https://en.wikipedia.org/wiki/Material_implication_(rule_of_inference)</summary>
        /// <param name="assumption">The assumption node</param>
        /// <param name="conclusion">The conclusion node</param>
        /// <returns>Implies node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Implication(Entity assumption, Entity conclusion) => assumption.Implies(conclusion);

        /// <summary>https://en.wikipedia.org/wiki/Exclusive_or#:~:text=Exclusive%20or%20or%20exclusive%20disjunction,⊕%2C%20↮%2C%20and%20≢.</summary>
        /// <param name="a">Left argument node of which Exclusive disjunction function will be taken</param>
        /// <param name="b">Right argument node of which Exclusive disjunction function will be taken</param>
        /// <returns>Xor node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity ExclusiveDisjunction(Entity a, Entity b) => a ^ b;

        /// <summary>
        /// Do NOT confuse it with Equation
        /// </summary>
        /// <param name="a">Left argument node of which Equality function will be taken</param>
        /// <param name="b">Right argument node of which Equality disjunction function will be taken</param>
        /// <returns>An Equals node</returns>
        public static Entity Equality(Entity a, Entity b) => a.Equalizes(b);

        /// <param name="a">Left argument node of which the greater than node will be taken</param>
        /// <param name="b">Right argument node of which the greater than node function will be taken</param>
        /// <returns>A node</returns>
        public static Entity GreaterThan(Entity a, Entity b) => a > b;

        /// <param name="a">Left argument node of which the less than node will be taken</param>
        /// <param name="b">Right argument node of which the less than node function will be taken</param>
        /// <returns>A node</returns>
        public static Entity LessThan(Entity a, Entity b) => a < b;

        /// <param name="a">Left argument node of which the greter than or equal node will be taken</param>
        /// <param name="b">Right argument node of which the greater than or equal node function will be taken</param>
        /// <returns>A node</returns>
        public static Entity GreaterOrEqualThan(Entity a, Entity b) => a >= b;

        /// <param name="a">Left argument node of which the less than or equal node will be taken</param>
        /// <param name="b">Right argument node of which the less than or equal node function will be taken</param>
        /// <returns>A node</returns>
        public static Entity LessOrEqualThan(Entity a, Entity b) => a <= b;

        /// <param name="a">Left argument node of which the union set node will be taken</param>
        /// <param name="b">Right argument node of which the union set node will be taken</param>
        /// <returns>A node</returns>
        public static Set Union(Entity a, Entity b) => a.Unite(b);

        /// <param name="a">Left argument node of which the intersection set node will be taken</param>
        /// <param name="b">Right argument node of which the intersection set node will be taken</param>
        /// <returns>A node</returns>
        public static Set Intersection(Entity a, Entity b) => a.Intersect(b);

        /// <param name="a">
        /// Left argument node of which the set subtraction node will be taken
        /// That is, the resulting set of set subtraction is necessarily superset of this set
        /// </param>
        /// <param name="b">
        /// Right argument node of which the set subtraction set node will be taken
        /// That is, there is no element in the resulting set that belong to this one
        /// </param>
        /// <returns>A node</returns>
        public static Set SetSubtraction(Entity a, Entity b) => a.SetSubtract(b);

        /// <summary>Creates an instance of <see cref="Variable"/>.</summary>
        /// <param name="name">The name of the <see cref="Variable"/> which equality is based on.</param>
        /// <returns>Variable node</returns>
        public static Variable Var(string name) => name;

        // List of public constants
        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// The e constant
        /// <a href="https://en.wikipedia.org/wiki/E_(mathematical_constant)"/>
        /// </summary>
        public static readonly Variable e = Variable.e;
        // ReSharper disable once InconsistentNaming

        /// <summary>
        /// The imaginary one
        /// <a href="https://en.wikipedia.org/wiki/Imaginary_unit"/>
        /// </summary>
        public static readonly Complex i = Complex.ImaginaryOne;

        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// The pi constant
        /// <a href="https://en.wikipedia.org/wiki/Pi"/>
        /// </summary>
        public static readonly Variable pi = Variable.pi;

        // Undefined
        /// <summary>
        /// That is both undefined and indeterminite
        /// Any operation on NaN returns NaN
        /// </summary>
        public static readonly Entity NaN = Real.NaN;

        /// <summary>Converts a <see cref="string"/> to an expression</summary>
        /// <param name="expr"><see cref="string"/> expression, for example, <code>"2 * x + 3 + sqrt(x)"</code></param>
        /// <param name="useCache">By default is true, it boosts performance if you have multiple uses of the same string,
        /// for example, 
        /// Entity expr = (Entity)"+oo" + "x".Limit("x", "+oo") * "+oo";
        /// First occurance will be parsed, others will be replaced with the cached entity
        /// </param>
        /// <returns>The parsed expression</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity FromString(string expr, bool useCache) => 
            useCache ? stringToEntityCache.GetValue(expr, key => Parser.Parse(key)) : Parser.Parse(expr);

        /// <summary>Converts a <see cref="string"/> to an expression</summary>
        /// <param name="expr"><see cref="string"/> expression, for example, <code>"2 * x + 3 + sqrt(x)"</code></param>
        /// <returns>The parsed expression</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity FromString(string expr) => FromString(expr, useCache: true);
        internal static ConditionalWeakTable<string, Entity> stringToEntityCache = new();

        /// <summary>Translates a <see cref="Number"/> in base 10 into base <paramref name="N"/></summary>
        /// <param name="num">A <see cref="Real"/> in base 10 to be translated into base <paramref name="N"/></param>
        /// <param name="N">The base to translate the number into</param>
        /// <returns>A <see cref="string"/> with the number in the required base</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToBaseN(Number.Real num, int N) => BaseConversion.ToBaseN(num.EDecimal, N);

        /// <summary>Translates a number in base <paramref name="N"/> into base 10</summary>
        /// <param name="num">A <see cref="Real"/> in base <paramref name="N"/> to be translated into base 10</param>
        /// <param name="N">The base to translate the number from</param>
        /// <returns>The <see cref="Real"/> in base 10</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Number.Real FromBaseN(string num, int N) => BaseConversion.FromBaseN(num, N);

        /// <returns>
        /// The <a href="https://en.wikipedia.org/wiki/LaTeX">LaTeX</a> representation of the argument
        /// </returns>
        /// <param name="latexiseable">
        /// Any element (<see cref="Entity"/>, <see cref="Set"/>, etc.) that can be represented in LaTeX
        /// </param>
        public static string Latex(ILatexiseable latexiseable) => latexiseable.Latexise();

        /// <summary>
        /// <para>All operations for <see cref="Number"/> and its derived classes are available from here.</para>
        ///
        /// These methods represent the only possible way to explicitly create numeric instances.
        /// It will automatically downcast the result for you,
        /// so <code>Number.Create(1.0);</code> is an <see cref="Integer"/>.
        /// To avoid it, you may temporarily disable it
        /// <code>
        ///  MathS.Settings.DowncastingEnabled.As(false, () =>
        /// {
        ///    var yourNum = Number.Create(1.0);
        /// });
        /// </code>
        /// and the result will be a <see cref="Real"/>.
        /// </summary>
        public static class Numbers
        {
            /// <summary>Creates an instance of <see cref="Complex"/> from a <see cref="NumericsComplex"/></summary>
            /// <param name="value">A value of type <see cref="NumericsComplex"/></param>
            /// <returns>The resulting <see cref="Complex"/></returns>
            public static Complex Create(NumericsComplex value) =>
                Create(EDecimal.FromDouble(value.Real), EDecimal.FromDouble(value.Imaginary));

            /// <summary>Creates an instance of <see cref="Integer"/> from a <see cref="long"/></summary>
            /// <param name="value">A value of type <see cref="long"/> (signed 64-bit integer)</param>
            /// <returns>The resulting <see cref="Integer"/></returns>
            public static Integer Create(long value) => Integer.Create(value);

            /// <summary>Creates an instance of <see cref="Integer"/> from an <see cref="EInteger"/></summary>
            /// <param name="value">A value of type <see cref="EInteger"/></param>
            /// <returns>The resulting <see cref="Integer"/></returns>
            public static Integer Create(EInteger value) => Integer.Create(value);

            /// <summary>Creates an instance of <see cref="Integer"/> from an <see cref="int"/></summary>
            /// <param name="value">A value of type <see cref="int"/> (signed 32-bit integer)</param>
            /// <returns>The resulting <see cref="Integer"/></returns>
            public static Integer Create(int value) => Integer.Create(value);

            /// <summary>Creates an instance of <see cref="Rational"/> from two <see cref="EInteger"/>s</summary>
            /// <param name="numerator">Numerator of type <see cref="EInteger"/></param>
            /// <param name="denominator">Denominator of type <see cref="EInteger"/></param>
            /// <returns>
            /// The resulting <see cref="Rational"/>
            /// </returns>
            public static Rational CreateRational(EInteger numerator, EInteger denominator)
                => Rational.Create(numerator, denominator);

            /// <summary>Creates an instance of <see cref="Rational"/> from an <see cref="ERational"/></summary>
            /// <param name="rational">A value of type <see cref="ERational"/></param>
            /// <returns>The resulting <see cref="Rational"/></returns>
            public static Rational Create(ERational rational) => Rational.Create(rational);

            /// <summary>Creates an instance of <see cref="Real"/> from an <see cref="EDecimal"/></summary>
            /// <param name="value">A value of type <see cref="EDecimal"/></param>
            /// <returns>The resulting <see cref="Real"/></returns>
            public static Real Create(EDecimal value) => Real.Create(value);

            /// <summary>Creates an instance of <see cref="Real"/> from a <see cref="double"/></summary>
            /// <param name="value">A value of type <see cref="double"/> (64-bit floating-point number)</param>
            /// <returns>The resulting <see cref="Real"/></returns>
            public static Real Create(double value) => Real.Create(EDecimal.FromDouble(value));

            /// <summary>
            /// Creates an instance of <see cref="Complex"/> from two <see cref="EDecimal"/>s
            /// </summary>
            /// <param name="re">
            /// Real part of the desired <see cref="Complex"/> of type <see cref="EDecimal"/>
            /// </param>
            /// <param name="im">
            /// Imaginary part of the desired <see cref="Complex"/> of type <see cref="EDecimal"/>
            /// </param>
            /// <returns>The resulting <see cref="Complex"/></returns>
            public static Complex Create(EDecimal re, EDecimal im) => Complex.Create(re, im);
        }

        /// <summary>Classes and functions related to matrices are defined here</summary>
        public static class Matrices
        {
            /// <summary>
            /// Creates an instance of <see cref="Tensor"/> that is a matrix.
            /// Usage example:
            /// <code>
            /// var t = MathS.Matrix(5, 3,<br/>
            /// <list type="bullet"><list type="bullet"><list type="bullet"><list type="bullet">
            ///     10, 11, 12,<br/>
            ///     20, 21, 22,<br/>
            ///     30, 31, 32,<br/>
            ///     40, 41, 42,<br/>
            ///     50, 51, 52);
            /// </list></list></list></list>
            /// </code>
            /// creates 5×3 matrix with the appropriate elements
            /// </summary>
            /// <param name="rows">Number of rows (first axis)</param>
            /// <param name="columns">Number of columns (second axis)</param>
            /// <param name="values">
            /// Array of values of the matrix so that its length is equal to
            /// the product of <paramref name="rows"/> and <paramref name="columns"/>
            /// </param>
            /// <returns>A two-dimensional <see cref="Tensor"/> which is a matrix</returns>
            public static Tensor Matrix(int rows, int columns, params Entity[] values) =>
                new(GenTensor.CreateMatrix(rows, columns, (x, y) => values[x * columns + y]));

            /// <summary>Creates an instance of <see cref="Tensor"/> that is a matrix.</summary>
            /// <param name="values">A two-dimensional array of values</param>
            /// <returns>A two-dimensional <see cref="Tensor"/> which is a matrix</returns>
            public static Tensor Matrix(Entity[,] values) => new(GenTensor.CreateMatrix(values));

            /// <summary>Creates an instance of <see cref="Tensor"/> that is a vector.</summary>
            /// <param name="values">The cells of the <see cref="Tensor"/></param>
            /// <returns>A one-dimensional <see cref="Tensor"/> which is a vector</returns>
            public static Tensor Vector(params Entity[] values) => new(GenTensor.CreateVector(values));

            /// <summary>Returns the dot product of two <see cref="Tensor"/>s that are matrices.</summary>
            /// <param name="A">First matrix (its width is the result's width)</param>
            /// <param name="B">Second matrix (its height is the result's height)</param>
            /// <returns>A two-dimensional <see cref="Tensor"/> (matrix) as a result of symbolic multiplication</returns>
            [Obsolete("Use MatrixMultiplication instead")]
            public static Tensor DotProduct(Tensor A, Tensor B) => new(GenTensor.MatrixMultiply(A.InnerTensor, B.InnerTensor));

            /// <summary>Returns the dot product of two <see cref="Tensor"/>s that are matrices.</summary>
            /// <param name="A">First matrix (its width is the result's width)</param>
            /// <param name="B">Second matrix (its height is the result's height)</param>
            /// <returns>A two-dimensional <see cref="Tensor"/> (matrix) as a result of symbolic multiplication</returns>
            public static Tensor MatrixMultiplication(Tensor A, Tensor B) => new(GenTensor.TensorMatrixMultiply(A.InnerTensor, B.InnerTensor));

            /// <summary>Returns the scalar product of two <see cref="Tensor"/>s that are vectors with the same length.</summary>
            /// <param name="a">First vector (order does not matter)</param>
            /// <param name="b">Second vector</param>
            /// <returns>The resulting scalar which is an <see cref="Entity"/> and not a <see cref="Tensor"/></returns>
            public static Entity ScalarProduct(Tensor a, Tensor b) => GenTensor.VectorDotProduct(a.InnerTensor, b.InnerTensor);

            /// <summary>
            /// Creates a closed interval (segment)
            /// </summary>
            public static Interval Interval(Entity left, Entity right) => new Interval(left, true, right, true);

            /// <summary>
            /// Creates an interval with custom endings
            /// </summary>
            public static Interval Interval(Entity left, bool leftClosed, Entity right, bool rightClosed) => new Interval(left, leftClosed, right, rightClosed);
        }

        /// <summary>
        /// A couple of settings allowing you to set some preferences for AM's algorithms
        /// To use these settings the syntax is
        /// <code>
        /// MathS.Settings.SomeSetting.As(5 /* Here you set a value to the setting */, () => { ... /* your code */ });
        /// </code>
        /// </summary>
        public static partial class Settings
        {
            /// <summary>
            /// This class for configuring some internal mechanisms from outside
            /// </summary>
            /// <typeparam name="T">
            /// Those configurations can be of different types
            /// </typeparam>
            public sealed class Setting<T> where T : notnull
            {
                internal Setting(T defaultValue) { Value = defaultValue; Default = defaultValue; }

                /// <summary>
                /// For example,
                /// <code>
                /// MathS.Settings.Precision.As(100, () => { /* some code considering precision = 100 */ });
                /// </code>
                /// </summary>
                /// <param name="value">New value that will be automatically reverted after action is done</param>
                /// <param name="action">What should be done under this setting</param>
                public void As(T value, Action action)
                {
                    var previousValue = Value;
                    Value = value;
                    try
                    { 
                        action(); 
                    } 
                    finally
                    {
                        Value = previousValue;
                    }
                }

                /// <summary>
                /// For example,
                /// <code>
                /// var res = MathS.Settings.Precision.As(100, () => { /* some code considering precision = 100 */ return 4; });
                /// </code>
                /// </summary>
                /// <param name="value">New value that will be automatically reverted after action is done</param>
                /// <param name="action">What should be done under this setting</param>
                public TReturnType As<TReturnType>(T value, Func<TReturnType> action)
                {
                    var previousValue = Value;
                    Value = value;
                    try
                    {
                        return action(); 
                    } 
                    finally
                    { 
                        Value = previousValue;
                    }
                }

                /// <summary>
                /// An implicit operator so that one does not have to call <see cref="Value"/>
                /// </summary>
                /// <param name="s">The setting</param>
                public static implicit operator T(Setting<T> s) => s.Value;

                /// <summary>
                /// An implicit operator so that one does not have to call the ctor
                /// </summary>
                /// <param name="a">The value</param>
                public static implicit operator Setting<T>(T a) => new(a);

                /// <summary>
                /// Overriden ToString so that one could see the value of the setting
                /// (if overriden)
                /// </summary>
                public override string ToString() => Value.ToString();

                /// <summary>
                /// The current value of the setting
                /// </summary>
                public T Value { get; private set; }

                /// <summary>
                /// The default value of the setting
                /// </summary>
                public T Default { get; }
            }

            /// <summary>
            /// That is how we perform newton solving when no analytical solution was found
            /// in <see cref="Entity.Solve(Variable)"/> and <see cref="Entity.SolveEquation(Variable)"/>
            /// </summary>
            public sealed record NewtonSetting
            {
                /// <summary>
                /// The point where we start going from
                /// </summary>
                public (EDecimal Re, EDecimal Im) From { get; init; } = (-10, -10);

                /// <summary>
                /// The point after which we do not perform seach
                /// </summary>
                public (EDecimal Re, EDecimal Im) To { get; init; } = (10, 10);

                /// <summary>
                /// The number of steps to go through for real and for complex part
                /// </summary>
                public (int Re, int Im) StepCount { get; init; } = (10, 10);

                /// <summary>
                /// How precise the result is required to be. The higher, the longer 
                /// the algorithm takes to return the result
                /// </summary>
                public int Precision { get; init; } = 30;
            }
            /// <summary>
            /// Enables downcasting. Not recommended to turn off, disabling might be only useful for some calculations
            /// </summary>
            public static Setting<bool> DowncastingEnabled => downcastingEnabled ??= true;
            [ThreadStatic] private static Setting<bool>? downcastingEnabled;

            /// <summary>
            /// Amount of iterations allowed for attempting to cast to a rational
            /// The more iterations, the larger fraction could be calculated
            /// </summary>
            public static Setting<int> FloatToRationalIterCount => floatToRationalIterCount ??= 15;
            [ThreadStatic] private static Setting<int>? floatToRationalIterCount;

            /// <summary>
            /// If a numerator or denominator is too large, it's suspended to better keep the real number instead of casting
            /// </summary>
            public static Setting<EInteger> MaxAbsNumeratorOrDenominatorValue =>
                maxAbsNumeratorOrDenominatorValue ??= EInteger.FromInt32(100000000);
            [ThreadStatic] private static Setting<EInteger>? maxAbsNumeratorOrDenominatorValue;

            /// <summary>
            /// Sets threshold for comparison
            /// For example, if you don't need precision higher than 6 digits after .,
            /// you can set it to 1.0e-6 so 1.0000000 == 0.9999999
            /// </summary>
            public static Setting<EDecimal> PrecisionErrorCommon => precisionErrorCommon ??= EDecimal.Create(1, -6);
            [ThreadStatic] private static Setting<EDecimal>? precisionErrorCommon;


            /// <summary>
            /// Numbers whose absolute value is less than PrecisionErrorZeroRange are considered zeros
            /// </summary>
            public static Setting<EDecimal> PrecisionErrorZeroRange => precisionErrorZeroRange ??= EDecimal.Create(1, -16);
            [ThreadStatic] private static Setting<EDecimal>? precisionErrorZeroRange;

            /// <summary>
            /// If you only need analytical solutions and an empty set if no analytical solutions were found, disable Newton's method
            /// </summary>
            public static Setting<bool> AllowNewton => allowNewton ??= true;
            [ThreadStatic] private static Setting<bool>? allowNewton;

            /// <summary>
            /// Criteria for simplifier so you could control which expressions are considered easier by you
            /// </summary>
            public static Setting<Func<Entity, int>> ComplexityCriteria =>
                complexityCriteria ??= new Func<Entity, int>(expr =>
                {
                    // Number of nodes
                    var res = expr.Complexity;

                    // Number of variables
                    res += expr.Nodes.Count(entity => entity is Variable);

                    // Number of divides
                    res += expr.Nodes.Count(entity => entity is Divf) / 2;

                    // Number of negative powers
                    res += expr.Nodes.Count(entity => entity is Powf(_, Real { IsNegative: true })) * 4;

                    // Number of negative reals
                    res += expr.Nodes.Count(entity => entity is Real { IsNegative: true }) * 3 /* to outweigh number of nodes */;

                    // 0 < x is bad. x > 0 is good.
                    res += expr.Nodes.Count(entity => entity is ComparisonSign && entity.DirectChildren[0] == 0);

                    return res;
                });
            [ThreadStatic] private static Setting<Func<Entity, int>>? complexityCriteria;

            /// <summary>
            /// Settings for the Newton-Raphson's root-search method
            /// e.g.
            /// <code>
            /// MathS.Settings.NewtonSolver.As(new NewtonSetting {
            ///     From = (-10, -10),
            ///     To = (10, 10),
            ///     StepCount = (10, 10),
            ///     Precision = 30
            /// }, () =>
            /// ...
            /// );
            /// </code>
            /// </summary>
            public static Setting<NewtonSetting> NewtonSolver => newtonSolver ??= new NewtonSetting();
            [ThreadStatic] private static Setting<NewtonSetting>? newtonSolver;

            /// <summary>
            /// The maximum number of linear children of an expression in polynomial solver
            /// considering that there's no more than 1 children with the required variable, for example:
            /// <list type="table">
            ///     <listheader>
            ///         <term>Expression</term>
            ///         <description>Complexity</description>
            ///     </listheader>
            ///     <item>
            ///         <term>(x + 2) ^ 2</term>
            ///         <description>3 [x2, 4x, 4]</description>
            ///     </item>
            ///     <item>
            ///         <term>x + 3 + a</term>
            ///         <description>2 [x, 3 + a]</description>
            ///     </item>
            ///     <item>
            ///         <term>(x + a)(b + c)</term>
            ///         <description>2 [(b + c)x, a(b + c)]</description>
            ///     </item>
            ///     <item>
            ///         <term>(x + 3 + a) / (x + 3)</term>
            ///         <description>2 [x / (x + 3), (3 + a) / (x + 3)]</description>
            ///     </item>
            ///     <item>
            ///         <term>x2 + x + 1</term>
            ///         <description>3 [x2, x, 1]</description>
            ///     </item>
            /// </list>
            /// </summary>
            public static Setting<int> MaxExpansionTermCount => maxExpansionTermCount ??= 50;
            [ThreadStatic] private static Setting<int>? maxExpansionTermCount;

            /// <summary>
            /// Settings for <see cref="EDecimal"/> precisions of <a href="https://github.com/peteroupc/Numbers">PeterO.Numbers</a>
            /// </summary>
            public static Setting<EContext> DecimalPrecisionContext =>
                decimalPrecisionContext ??= new EContext(100, ERounding.HalfUp, -100, 1000, false);
            [ThreadStatic] private static Setting<EContext>? decimalPrecisionContext;
        }

        /// <summary>Returns an <see cref="Entity"/> in polynomial order if possible</summary>
        /// <param name="expr">The unordered <see cref="Entity"/></param>
        /// <param name="variable">The variable of the polynomial</param>
        /// <param name="dst">The ordered result</param>
        /// <returns>
        /// <see langword="true"/> if success,
        /// <see langword="false"/> otherwise (<paramref name="dst"/> will be <see langword="null"/>)
        /// </returns>
        public static bool TryPolynomial(Entity expr, Variable variable,
            [System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
            out Entity? dst) => Simplificator.TryPolynomial(expr, variable, out dst);

        /// <returns>sympy interpretable format</returns>
        /// <param name="expr">An <see cref="Entity"/> representing an expression</param>
        public static string ToSympyCode(Entity expr)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("import sympy\n\n");
            foreach (var f in expr.Vars)
                sb.Append($"{f.Stringize()} = sympy.Symbol('{f.Stringize()}')\n");
            sb.Append('\n');
            sb.Append("expr = ").Append(expr.ToSymPy());
            return sb.ToString();
        }

        /// <summary>
        /// Functions and classes related to sets defined here
        /// 
        /// Class <see cref="Set"/> defines true mathematical sets
        /// It can be empty,
        /// it can contain <see cref="OneElementPiece"/>s,
        /// it can contain <see cref="Core.Interval"/>s etc.
        /// It supports intersection (with & operator), union (with | operator),
        ///             subtraction (with - operator) as well as inversion (with ! operator).
        /// </summary>
        public static class Sets
        {
            /// <summary>Creates an instance of an empty <see cref="Set"/></summary>
            /// <returns>A <see cref="Set"/> with no elements</returns>
            public static Set Empty => Set.Empty;

            /// <returns>A set of all Complexes/>s</returns>
            public static Set C => SpecialSet.Create(Domain.Complex);

            /// <returns>A set of all Reals/>s</returns>
            public static Set R => SpecialSet.Create(Domain.Real);

            /// <returns>A set of all Rationals/>s</returns>
            public static Set Q => SpecialSet.Create(Domain.Rational);

            /// <returns>A set of all Integers/></returns>
            public static Set Z => SpecialSet.Create(Domain.Integer);

            /// <summary>
            /// Creates a <see cref="FiniteSet"/> with given elements
            /// </summary>
            public static FiniteSet Finite(params Entity[] entities) => new FiniteSet(entities);

            /// <summary>
            /// Creates a <see cref="FiniteSet"/> with given elements
            /// </summary>
            public static FiniteSet Finite(List<Entity> entities) => new FiniteSet((IEnumerable<Entity>)entities);

            /// <summary>
            /// Creates a closed interval
            /// </summary>
            public static Interval Interval(Entity from, Entity to) => new(from, true, to, true);

            /// <summary>
            /// Creates an interval where <paramref name="leftClosed"/> shows whether <paramref name="from"/> is included,
            /// <paramref name="rightClosed"/> shows whether <paramref name="to"/> included.
            /// </summary>
            public static Interval Interval(Entity from, bool leftClosed, Entity to, bool rightClosed) => new(from, leftClosed, to, rightClosed);

            /// <summary>
            /// Creates a node of whether the given element belongs to the given set
            /// </summary>
            /// <returns>A node</returns>
            public static Entity ElementInSet(Entity element, Entity set)
                => element.In(set);
        }

        /// <summary>
        /// Implements necessary functions for symbolic computation of limits, derivatives and integrals
        /// </summary>
        public static class Compute
        {
            /// <summary>
            /// If possible, analytically computes the limit of <paramref name="expr"/>
            /// if <paramref name="var"/> approaches to <paramref name="approachDestination"/>
            /// from one of two sides (left and right).
            /// Returns <see langword="null"/> otherwise.
            /// </summary>
            [Obsolete("Now these functions are available as non-static methods at Entity")]
            public static Entity Limit(Entity expr, Variable var, Entity approachDestination,
                ApproachFrom direction)
                => expr.Limit(var, approachDestination, direction);

            /// <summary>
            /// If possible, analytically computes the limit of <paramref name="expr"/>
            /// if <paramref name="var"/> approaches to <paramref name="approachDestination"/>.
            /// Returns <see langword="null"/> otherwise or if limits from left and right sides differ.
            /// </summary>
            [Obsolete("Now these functions are available as non-static methods at Entity")]
            public static Entity Limit(Entity expr, Variable var, Entity approachDestination)
                => expr.Limit(var, approachDestination, ApproachFrom.BothSides);

            /// <summary>Derives over <paramref name="x"/> <paramref name="power"/> times</summary>
            [Obsolete("Now these functions are available as non-static methods at Entity")]
            public static Entity Derivative(Entity expr, Variable x, EInteger power)
            {
                var ent = expr;
                for (var _ = 0; _ < power; _++)
                    ent = Derivative(ent, x);
                return ent;
            }

            /// <summary>Derivation over a variable (without simplification)</summary>
            /// <param name="expr">The expression to find derivative over</param>
            /// <param name="x">The variable to derive over</param>
            /// <returns>The derived result</returns>
            [Obsolete("Now these functions are available as non-static methods at Entity")]
            public static Entity Derivative(Entity expr, Variable x) => expr.Differentiate(x);

            /// <summary>Integrates over <paramref name="x"/> <paramref name="power"/> times</summary>
            [Obsolete("Now these functions are available as non-static methods at Entity")]
            public static Entity Integral(Entity expr, Variable x, EInteger power)
            {
                var ent = expr;
                for (var _ = 0; _ < power; _++)
                    ent = Integral(ent, x);
                return ent;
            }

            /// <summary>Integrates over a variable (without simplification)</summary>
            /// <param name="expr">The expression to be integrated over <paramref name="x"/></param>
            /// <param name="x">The variable to integrate over</param>
            /// <returns>The integrated result</returns>
            [Obsolete("Now these functions are available as non-static methods at Entity")]
            public static Entity Integral(Entity expr, Variable x) 
                => expr.Integrate(x);

            /// <summary>
            /// Returns a value of a definite integral of a function. Only works for one-variable functions
            /// </summary>
            /// <param name="expr">The expression to be numerically integrated over <paramref name="x"/></param>
            /// <param name="x">Variable to integrate over</param>
            /// <param name="from">The complex lower bound for integrating</param>
            /// <param name="to">The complex upper bound for integrating</param>
            [Obsolete("Now these functions are available as non-static methods at Entity")]
            public static Complex DefiniteIntegral(Entity expr, Variable x, (EDecimal Re, EDecimal Im) from, (EDecimal Re, EDecimal Im) to) =>
                Integration.Integrate(expr, x, from, to, 100);

            /// <summary>
            /// Returns a value of a definite integral of a function. Only works for one-variable functions
            /// </summary>
            /// <param name="expr">The function to be numerically integrated</param>
            /// <param name="x">Variable to integrate over</param>
            /// <param name="from">The real lower bound for integrating</param>
            /// <param name="to">The real upper bound for integrating</param>
            [Obsolete("Now these functions are available as non-static methods at Entity")]
            public static Complex DefiniteIntegral(Entity expr, Variable x, EDecimal from, EDecimal to) =>
                Integration.Integrate(expr, x, (from, 0), (to, 0), 100);

            /// <summary>
            /// Returns a value of a definite integral of a function. Only works for one-variable functions
            /// </summary>
            /// <param name="expr">The function to be numerically integrated</param>
            /// <param name="x">Variable to integrate over</param>
            /// <param name="from">The complex lower bound for integrating</param>
            /// <param name="to">The complex upper bound for integrating</param>
            /// <param name="stepCount">Accuracy (initially, amount of iterations)</param>
            [Obsolete("Now these functions are available as non-static methods at Entity")]
            public static Complex DefiniteIntegral(Entity expr, Variable x, (EDecimal Re, EDecimal Im) from, (EDecimal Re, EDecimal Im) to, int stepCount) =>
                Integration.Integrate(expr, x, from, to, stepCount);
        }
        /// <summary>
        /// Hangs your <see cref="Entity"/> to a derivative node
        /// (to evaluate instead use <see cref="Compute.Derivative(Entity, Variable)"/>)
        /// </summary>
        /// <param name="expr">Expression to be hung</param>
        /// <param name="var">Variable over which derivative is taken</param>
        public static Entity Derivative(Entity expr, Entity var) => new Derivativef(expr, var, 1);

        /// <summary>
        /// Hangs your <see cref="Entity"/> to a derivative node
        /// (to evaluate instead use <see cref="Compute.Derivative(Entity, Variable)"/>)
        /// </summary>
        /// <param name="expr">Expression to be hung</param>
        /// <param name="var">Variable over which derivative is taken</param>
        /// <param name="power">Number of times derivative is taken. Only integers will be simplified or evaluated.</param>
        public static Entity Derivative(Entity expr, Entity var, int power) => new Derivativef(expr, var, power);

        /// <summary>
        /// Hangs your entity to an integral node
        /// (to evaluate instead use <see cref="Compute.Integral(Entity, Variable)"/>)
        /// </summary>
        /// <param name="expr">Expression to be hung</param>
        /// <param name="var">Variable over which integral is taken</param>
        public static Entity Integral(Entity expr, Entity var) => new Integralf(expr, var, 1);

        /// <summary>
        /// Hangs your entity to an integral node
        /// (to evaluate instead use <see cref="Compute.Integral(Entity, Variable)"/>)
        /// </summary>
        /// <param name="expr">Expression to be hung</param>
        /// <param name="var">Variable over which integral is taken</param>
        /// <param name="power">Number of times integral is taken. Only integers will be simplified or evaluated.</param>
        public static Entity Integral(Entity expr, Entity var, int power) => new Integralf(expr, var, power);

        /// <summary>
        /// Hangs your entity to a limit node
        /// (to evaluate instead use <see cref="Compute.Limit(Entity, Variable, Entity)"/>)
        /// </summary>
        /// <param name="expr">Expression to be hung</param>
        /// <param name="var">Variable over which limit is taken</param>
        /// <param name="dest">Where <paramref name="var"/> approaches (could be finite or infinite)</param>
        /// <param name="approach">From where it approaches</param>
        public static Entity Limit(Entity expr, Entity var, Entity dest, ApproachFrom approach = ApproachFrom.BothSides)
            => new Limitf(expr, var, dest, approach);

        /// <summary>Some non-symbolic constants</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles",
            Justification = "Lowercase constants as written in Mathematics")]
        public static class DecimalConst
        {
            /// <summary><a href="https://en.wikipedia.org/wiki/Pi"/></summary>
            public static EDecimal pi =>
                NumbersExtensions.ConstantCache.Lookup(Settings.DecimalPrecisionContext).Pi;

            /// <summary><a href="https://en.wikipedia.org/wiki/E_(mathematical_constant)"/></summary>
            public static EDecimal e =>
                NumbersExtensions.ConstantCache.Lookup(Settings.DecimalPrecisionContext).E;
        }

        /// <summary>
        /// Some operations on booleans are stored here
        /// </summary>
        public static class Boolean
        {

            /// <summary>
            /// Combines all possible values of <paramref name="variables"/>
            /// and has the last column as the result of the function
            /// </summary>
            public static Tensor? BuildTruthTable(Entity expression, params Variable[] variables)
                => BooleanSolver.BuildTruthTable(expression, variables);


            /// <summary>
            /// Creates a boolean
            /// </summary>
            public static Entity.Boolean Create(bool b)
                => Entity.Boolean.Create(b);
        }

        /// <summary>
        /// Some additional functions that would be barely
        /// ever used by the user, but kept for "just in case" as public
        /// </summary>
        public static class Utils
        {
            /// <summary>
            /// Performs the expansion operation over the given variable
            /// </summary>
            public static Entity SmartExpandOver(Entity expr, Variable x)
            {
                var linChildren = Sumf.LinearChildren(expr);
                
                List<Entity> nodes = new();

                foreach (var linChild in linChildren)
                {
                    var children = TreeAnalyzer.SmartExpandOver(linChild, n => n.ContainsNode(n));
                    if (children is null)
                        return expr;
                    nodes.AddRange(children);
                }

                return TreeAnalyzer.MultiHangBinary(nodes, (a, b) => a + b);
            }

            /// <summary>
            /// Extracts a polynomial with integer powers
            /// </summary>
            /// <param name="expr">From which to extract the polynomial</param>
            /// <param name="variable">Over which variable to extract the polynomial</param>
            /// <param name="dst">
            /// Where to put the dictionary, whose keys
            /// are powers, and values - coefficients
            /// </param>
            /// <returns>Whether the input expression is a valid polynomial</returns>
            public static bool TryGetPolynomial(Entity expr, Variable variable,
            [NotNullWhen(true)] out Dictionary<EInteger, Entity>? dst)
                => TreeAnalyzer.TryGetPolynomial(expr, variable, out dst);

            /// <summary>
            /// Extracts the linear coefficient and the bias over a variable
            /// a x + b
            /// </summary>
            /// <param name="expr">From which to extract the linear function</param>
            /// <param name="variable">Over which to extract</param>
            /// <param name="a">The linear coefficient</param>
            /// <param name="b">The bias</param>
            /// <returns>Whether the extract was successful</returns>
            public static bool TryGetPolyLinear(Entity expr, Variable variable,
            [NotNullWhen(true)] out Entity? a,
            [NotNullWhen(true)] out Entity? b)
                => TreeAnalyzer.TryGetPolyLinear(expr, variable, out a, out b);

            /// <summary>
            /// Extracts the quadratic coefficient, linear coefficient and the bias over a variable
            /// a x ^ 2 + b x + c
            /// </summary>
            /// <param name="expr">From which to extract the quadratic function</param>
            /// <param name="variable">Over which to extract</param>
            /// <param name="a">The quadratic coefficient</param>
            /// <param name="b">The linear coefficient</param>
            /// <param name="c">The bias</param>
            /// <returns>Whether the extract was successful</returns>
            public static bool TryGetPolyQuadratic(Entity expr, Variable variable,
            [NotNullWhen(true)] out Entity? a,
            [NotNullWhen(true)] out Entity? b,
            [NotNullWhen(true)] out Entity? c)
                => TreeAnalyzer.TryGetPolyQuadratic(expr, variable, out a, out b, out c);
        }

        /// <summary>
        /// You may need it to manually manage some issues
        /// </summary>
        public static class Unsafe
        {
            /// <summary>
            /// When you implicitly convert string to an Entity,
            /// it caches the result by the string's reference.
            /// If very strict about RAM usage, you can manually
            /// clean it (or use <see cref="MathS.FromString(string, bool)"/>
            /// instead and set the flag useCache to false)
            /// </summary>
            public static void ClearFromStringCache()
                => MathS.stringToEntityCache = new();

            /// <summary>
            /// Entities' properties are not initialized once
            /// a node is created. They are stored in another
            /// weak table, whose memory consumtion might
            /// be critical for some system. You can clean it
            /// </summary>
            public static void ClearEntityPropertyCache()
                => Entity.caches = new();
        }

        /// <summary>
        /// Although those functions might be used inside the library
        /// only, the user may want to use them for some reason
        /// </summary>
        public static class Internal
        {
            /// <summary>
            /// Checks if two expressions are equivalent if 
            /// <see cref="Entity.Simplify"/> does not give the
            /// expected response
            /// </summary>
            public static bool AreEqualNumerically(Entity expr1, Entity expr2, Entity[] checkPoints)
                => ExpressionNumerical.AreEqual(expr1, expr2, checkPoints);

            /// <summary>
            /// Checks if two expressions are equivalent if 
            /// <see cref="Entity.Simplify"/> does not give the
            /// expected response
            /// </summary>
            public static bool AreEqualNumerically(Entity expr1, Entity expr2)
                => ExpressionNumerical.AreEqual(expr1, expr2);

            /// <summary>
            /// Checkpoints for numerical equality check
            /// </summary>
            public static Setting<Entity[]> CheckPoints => checkPoints ??= new Entity[]
            {
                -100, -10, 1, 10, 100, 1.5,
                "-100 + i", "-10 + 2i", "30i"
            };
            private static Setting<Entity[]>? checkPoints;
        }
    }
}
