
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using PeterO.Numbers;
using AngouriMath.Core;
using AngouriMath.Functions;
using AngouriMath.Functions.Algebra;
using AngouriMath.Functions.Algebra.NumericalSolving;

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

        // Marking small enums with ": byte" is premature optimization and shouldn't be done: https://stackoverflow.com/q/648823/5429648

        [Flags]
        public enum Inequality
        {
            LessThan = 0b00,
            GreaterThan = 0b01,
            EqualsFlag = 0b10,
            LessEquals = 0b10,
            GreaterEquals = 0b11,
        }

        /// <summary>Will be added soon! Solves an inequality numerically</summary>
        /// <param name="inequality">
        /// This must only contain one variable, which is <paramref name="var"/>
        /// </param>
        /// <param name="var">The only variable</param>
        /// <param name="sign">The relation of the expression to zero.</param>
#pragma warning disable IDE0060 // Remove unused parameter
        public static Set SolveInequalityNumerically(Entity inequality, Variable var, Inequality sign)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            throw new NotSupportedException("Will be added soon");
#pragma warning disable 162
            return NumericalInequalitySolver.Solve(inequality, var, sign);
#pragma warning restore 162
        }

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
        /// <param name="@base">Base node of logarithm</param>
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

        /// <summary>Creates an instance of <see cref="Variable"/>.</summary>
        /// <param name="name">The name of the <see cref="Variable"/> which equality is based on.</param>
        /// <returns>Variable node</returns>
        public static Variable Var(string name) => name;

        // List of public constants
        // ReSharper disable once InconsistentNaming
        public static readonly Variable e = Variable.e;
        // ReSharper disable once InconsistentNaming
        public static readonly Complex i = Complex.ImaginaryOne;
        // ReSharper disable once InconsistentNaming
        public static readonly Variable pi = Variable.pi;

        /// <summary>Converts a <see cref="string"/> to an expression</summary>
        /// <param name="expr"><see cref="string"/> expression, for example, <code>"2 * x + 3 + sqrt(x)"</code></param>
        /// <returns>The parsed expression</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity FromString(string expr) => Parser.Parse(expr);

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
            public class Setting<T> where T : notnull
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
                    try { action(); } finally { Value = previousValue; }
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
                    try { return action(); } finally { Value = previousValue; }
                }

                public static implicit operator T(Setting<T> s) => s.Value;
                public static implicit operator Setting<T>(T a) => new(a);
                public override string ToString() => Value.ToString();
                public T Value { get; private set; }
                public T Default { get; }
            }
            public record NewtonSetting
            {
                public (EDecimal Re, EDecimal Im) From { get; init; } = (-10, -10);
                public (EDecimal Re, EDecimal Im) To { get; init; } = (10, 10);
                public (int Re, int Im) StepCount { get; init; } = (10, 10);
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
                    res += expr.Nodes.Count(entity => entity is Powf(_, Number.Real { IsNegative: true }));

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
                sb.Append($"{f} = sympy.Symbol('{f}')\n");
            sb.Append("\n");
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
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Set Empty() => new Set();

            /// <returns>A set of all <see cref="Complex"/>s</returns>
            public static Set C() => Set.C();

            /// <returns>A set of all <see cref="Real"/>s</returns>
            public static Set R() => Set.R();

            /// <summary>
            /// Creates a <see cref="Set"/> that you can fill with elements
            /// Later on, you may add an Interval if you wish
            /// </summary>
            public static Set Finite(params Entity[] entities) => Set.Finite(entities);

            /// <summary>
            /// Creates an interval. To modify it, use e.g.
            /// <see cref="Interval.SetLeftClosed(bool)"/>
            /// (see more alike functions in <see cref="Set"/> documentation)
            /// </summary>
            public static Interval Interval(Entity from, Entity to) => new(from, to);

            /// <summary>
            /// Creates an element for <see cref="Set"/>. One can be created implicitly, <code>Piece a = 3;</code>
            /// </summary>
            public static OneElementPiece Element(Entity element) => new OneElementPiece(element);
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
            public static Entity? Limit(Entity expr, Variable var, Entity approachDestination,
                ApproachFrom direction)
                => LimitFunctional.ComputeLimit(expr, var, approachDestination, direction);

            /// <summary>
            /// If possible, analytically computes the limit of <paramref name="expr"/>
            /// if <paramref name="var"/> approaches to <paramref name="approachDestination"/>.
            /// Returns <see langword="null"/> otherwise or if limits from left and right sides differ.
            /// </summary>
            public static Entity? Limit(Entity expr, Variable var, Entity approachDestination)
                => LimitFunctional.ComputeLimit(expr, var, approachDestination);

            /// <summary>Derives over <paramref name="x"/> <paramref name="power"/> times</summary>
            public static Entity? Derivative(Entity expr, Variable x, EInteger power)
            {
                var ent = expr;
                for (var _ = 0; _ < power; _++)
                    ent = ent is { } ? Derivative(ent, x) : ent;
                return ent;
            }

            /// <summary>Derivation over a variable (without simplification)</summary>
            /// <param name="x">The variable to derive over</param>
            /// <returns>The derived result</returns>
            public static Entity? Derivative(Entity expr, Variable x) => expr.Derive(x);

            /// <summary>Integrates over <paramref name="x"/> <paramref name="power"/> times</summary>
            public static Entity? Integral(Entity expr, Variable x, EInteger power)
            {
                var ent = expr;
                for (var _ = 0; _ < power; _++)
                    ent = ent is { } ? Integral(ent, x) : ent;
                return ent;
            }

            /// <summary>Integrates over a variable (without simplification)</summary>
            /// <param name="x">The variable to integrate over</param>
            /// <returns>The integrated result</returns>
#pragma warning disable IDE0060 // Remove unused parameter
            public static Entity? Integral(Entity expr, Variable x) =>
#pragma warning restore IDE0060 // Remove unused parameter
                throw new NotImplementedException("Integrals not implemented yet");
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
        public static Entity Derivative(Entity expr, Entity var, Entity power) => new Derivativef(expr, var, power);

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
        public static Entity Integral(Entity expr, Entity var, Entity power) => new Integralf(expr, var, power);

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
    }
}
