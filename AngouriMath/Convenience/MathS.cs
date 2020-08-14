
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
using System.Numerics;
using System.Linq;
using System.Runtime.CompilerServices;
using PeterO.Numbers;
using AngouriMath.Core;
using AngouriMath.Core.FromString;
using AngouriMath.Core.Numerix;
using AngouriMath.Core.Sys;
using AngouriMath.Limits;
using AngouriMath.Functions;
using AngouriMath.Functions.Algebra;
using AngouriMath.Functions.Algebra.InequalitySolver;

namespace AngouriMath
{
    using static Entity;
    using GenTensor = GenericTensor.Core.GenTensor<Entity, Entity.Tensor.EntityTensorWrapperOperations>;
    /// <summary>
    /// Use functions from this class
    /// </summary>
    /// If I need to add a function or operator (e.g. sin), I first pin this tab for reference :)
    /// To start, implement real number evaluation
    /// (Press F12 -> <see cref="PeterONumbersExtensions.Sin(PeterO.Numbers.EDecimal, PeterO.Numbers.EContext)"/>)
    /// then complex number evaluation
    /// (Press F12 -> <see cref="Core.Numerix.Number.Sin(Core.Numerix.ComplexNumber)"/>)
    ///
    /// Next, Add Wakeup to static ctor below
    ///  -> Copy static function class (Press F12 -> <see cref="Sinf.Wakeup()"/>)
    ///  -> Add instance method to Entity (Press F12 -> <see cref="Entity.Sin()"/>)).
    /// 
    /// After that,
    /// .Eval (Press F12 -> <see cref="Sinf.Eval(System.Collections.Generic.List{Entity})"/>)
    /// .Hang (Press F12 -> <see cref="new Sinf(Entity)"/>)
    /// .PHang (Press F12 -> <see cref="Sinf.PHang(Entity)"/>)
    /// .ToString (Press F12 -> <see cref="Sinf.Stringize(System.Collections.Generic.List{Entity})"/>)
    /// .Latex (Press F12 -> <see cref="Sinf.Latex(System.Collections.Generic.List{Entity})"/>)
    /// .Derive (Press F12 -> <see cref="Sinf.Derive(System.Collections.Generic.List{Entity}, Variable)"/>)
    /// .Simplify (Press F12 -> <see cref="Sinf.Simplify(System.Collections.Generic.List{Entity})"/>)
    /// To compilation (Press F12 -> <see cref="CompiledMathFunctions.func2Num"/>
    ///                                       ^ TODO: Replace numbers with enum ^
    ///                          and <see cref="FastExpression.Substitute(System.Numerics.Complex[])"/>)
    /// To From String Syntax Info goodStrings (Press F12 -> <see cref="Core.FromString.SyntaxInfo.goodStringsForFunctions"/>)
    /// To Pattern Replacer
    ///     (Press F12 -> <see cref="Patterns.TrigonometricRules"/>
    ///               and <see cref="Core.TreeAnalysis.TreeAnalyzer.Optimization.Trigonometry"/>
    ///               and <see cref="Core.TreeAnalysis.TreeAnalyzer.Optimization.ContainsTrigonometric(Entity)"/>
    ///               and <see cref="Functions.Evaluation.Simplification.Simplificator.Alternate(Entity, int)"/>)
    /// To static MathS() (Press F12 -> <see cref="Sin(Entity)"/>)
    /// To Analytical Solver (Press F12 -> <see cref="Functions.Algebra.Solver.Analytical.TrigonometricSolver"/>
    ///                                and <see cref="Functions.Algebra.AnalyticalSolving.AnalyticalSolver.Solve(Entity, Variable, Core.Set, bool)"/>)
    /// To TreeAnalyzer Optimization (Press F12 -> <see cref="Core.TreeAnalysis.TreeAnalyzer.Optimization.OptimizeTree(ref Entity)"/>)
    /// To ToSympyCode (Press F12 -> <see cref="Functions.Output.ToSympy.FuncTable"/>) (Tip: Enter 'import sympy' into https://live.sympy.org/ then test)
    /// 
    /// And finally, remember to add tests for all the new functionality!
    public static partial class MathS
    {
        /// <summary>
        /// Use it to solve equations
        /// </summary>
        /// <param name="equations">
        /// An array of <see cref="Entity"/> (or <see cref="string"/>s)
        /// the system consists of
        /// </param>
        /// <returns>
        /// An <see cref="EquationSystem"/> which can then be solved
        /// </returns>
        public static EquationSystem Equations(params Entity[] equations) => new EquationSystem(equations);

        /// <summary>
        /// Solves one equation over one variable
        /// </summary>
        /// <param name="equation">
        /// An equation that is assumed to equal 0
        /// </param>
        /// <param name="var">
        /// Variable whose values we are looking for
        /// </param>
        /// <returns>
        /// Returns a <see cref="Set"/> of possible values or intervals of values
        /// </returns>
        public static Set SolveEquation(Entity equation, Variable var)
            => EquationSolver.Solve(equation, var);

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

        /// <summary>
        /// Will be added soon!
        /// Solves an inequality numerically
        /// </summary>
        /// <param name="inequality">
        /// This must only contain one variable, which is <paramref name="var"/>
        /// </param>
        /// <param name="var">
        /// The only variable
        /// </param>
        /// <param name="sign">
        /// The relation of the expression to zero.
        /// </param>
        /// <returns></returns>
        public static Set SolveInequalityNumerically(Entity inequality, Variable var, Inequality sign)
        {
            throw new NotSupportedException("Will be added soon");
#pragma warning disable 162
            return NumericalInequalitySolver.Solve(inequality, var, sign);
#pragma warning restore 162
        }

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Trigonometric_functions"/>
        /// </summary>
        /// <param name="a">
        /// Argument node of sine
        /// </param>
        /// <returns>
        /// Sine node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Sin(Entity a) => new Sinf(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Trigonometric_functions"/>
        /// </summary>
        /// <param name="a">
        /// Argument node of cosine
        /// </param>
        /// <returns>
        /// Cosine node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Cos(Entity a) => new Cosf(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Logarithm"/>
        /// </summary>
        /// <param name="@base">
        /// Base node of logarithm
        /// </param>
        /// <param name="x">
        /// Argument node of logarithm
        /// </param>
        /// <returns>
        /// Logarithm node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Log(Entity @base, Entity x) => new Logf(@base, x);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Power_function"/>
        /// </summary>
        /// <param name="base">
        /// Base node of power
        /// </param>
        /// <param name="power">
        /// Argument node of power
        /// </param>
        /// <returns>
        /// Power node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Pow(Entity @base, Entity power) => new Powf(@base, power);

        /// <summary>
        /// Special case of <a href="https://en.wikipedia.org/wiki/Power_function"/>
        /// </summary>
        /// <param name="a">
        /// The argument of which square root will be taken
        /// </param>
        /// <returns>
        /// Power node with (1/2) as the power
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Sqrt(Entity a) => new Powf(a, RationalNumber.Create(1, 2));

        /// <summary>
        /// Special case of <a href="https://en.wikipedia.org/wiki/Power_function"/>
        /// </summary>
        /// <param name="a">
        /// The argument of which cube root will be taken
        /// </param>
        /// <returns>
        /// Power node with (1/3) as the power
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Cbrt(Entity a) => new Powf(a, RationalNumber.Create(1, 3));

        /// <summary>
        /// Special case of <a href="https://en.wikipedia.org/wiki/Power_function"/>
        /// </summary>
        /// <param name="a">
        /// Argument to be squared
        /// </param>
        /// <returns>
        /// Power node with 2 as the power
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Sqr(Entity a) => new Powf(a, 2);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Trigonometric_functions"/>
        /// </summary>
        /// <param name="a">
        /// Argument node of which tangent will be taken
        /// </param>
        /// <returns>
        /// Tangent node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Tan(Entity a) => new Tanf(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Trigonometric_functions"/>
        /// </summary>
        /// <param name="a">
        /// Argument node of which cotangent will be taken
        /// </param>
        /// <returns>
        /// Cotangent node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Cotan(Entity a) => new Cotanf(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Trigonometric_functions"/>
        /// </summary>
        /// <param name="a">
        /// Argument node of which secant will be taken
        /// </param>
        /// <returns>
        /// Reciprocal of cosine node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Sec(Entity a) => 1 / Cos(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Trigonometric_functions"/>
        /// </summary>
        /// <param name="a">
        /// Argument node of which cosecant will be taken
        /// </param>
        /// <returns>
        /// Reciprocal of sine node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Cosec(Entity a) => 1 / Sin(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Inverse_trigonometric_functions"/>
        /// </summary>
        /// <param name="a">
        /// Argument node of which arcsine will be taken
        /// </param>
        /// <returns>
        /// Arcsine node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Arcsin(Entity a) => new Arcsinf(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Inverse_trigonometric_functions"/>
        /// </summary>
        /// <param name="a">
        /// Argument node of which arccosine will be taken
        /// </param>
        /// <returns>
        /// Arccosine node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Arccos(Entity a) => new Arccosf(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Inverse_trigonometric_functions"/>
        /// </summary>
        /// <param name="a">
        /// Argument node of which arctangent will be taken
        /// </param>
        /// <returns>
        /// Arctangent node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Arctan(Entity a) => new Arctanf(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Inverse_trigonometric_functions"/>
        /// </summary>
        /// <param name="a">
        /// Argument node of which arccotangent will be taken
        /// </param>
        /// <returns>
        /// Arccotangent node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Arccotan(Entity a) => new Arccotanf(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Inverse_trigonometric_functions"/>
        /// </summary>
        /// <param name="a">
        /// Argument node of which arcsecant will be taken
        /// </param>
        /// <returns>
        /// Arccosine node with the reciprocal of the argument
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Arcsec(Entity a) => new Arccosf(1 / a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Inverse_trigonometric_functions"/>
        /// </summary>
        /// <param name="a">
        /// Argument node of which arccosecant will be taken
        /// </param>
        /// <returns>
        /// Arcsine node with the reciprocal of the argument
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Arccosec(Entity a) => new Arcsinf(1 / a);

        /// <summary>
        /// Is a special case of logarithm where the base equals
        /// <a href="https://en.wikipedia.org/wiki/E_(mathematical_constant)">e</a>:
        /// <a href="https://en.wikipedia.org/wiki/Natural_logarithm"/>
        /// </summary>
        /// <param name="a">
        /// Argument node of which natural logarithm will be taken
        /// </param>
        /// <returns>
        /// Logarithm node with base equal to e
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Ln(Entity a) => new Logf(e, a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Factorial"/>
        /// </summary>
        /// <param name="a">
        /// Argument node of which factorial will be taken
        /// </param>
        /// <returns>
        /// Factorial node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Factorial(Entity a) => new Factorialf(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Gamma_function"/>
        /// </summary>
        /// <param name="a">
        /// Argument node of which gamma function will be taken
        /// </param>
        /// <returns>
        /// Factorial node with one added to the argument
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Gamma(Entity a) => new Factorialf(a + 1);

        /// <summary>
        /// Creates an instance of <see cref="Variable"/>.
        /// </summary>
        /// <param name="name">
        /// The name of the variable.
        /// Variables of the same name are considered as the same variables.
        /// </param>
        /// <returns>
        /// Variable node
        /// </returns>
        public static Variable Var(string name) => new Variable(name);

        /// <summary>
        /// Creates a complex instance of <see cref="Number"/> (not <see cref="Number"/>!)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete("Use Number.Create or implicit construction instead")]
        public static Number Number(EDecimal a, EDecimal b) => ComplexNumber.Create(a, b);

        /// <summary>
        /// Creates a real instance of <see cref="Number"/> (not <see cref="Number"/>!)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete("Use Number.Create or implicit construction instead")]
        public static ComplexNumber Number(EDecimal a) => RealNumber.Create(a);

        // List of public constants
        // ReSharper disable once InconsistentNaming
        public static readonly Variable e = nameof(e);
        // ReSharper disable once InconsistentNaming
        public static readonly ComplexNumber i = ComplexNumber.ImaginaryOne;
        // ReSharper disable once InconsistentNaming
        public static readonly Variable pi = nameof(pi);

        /// <summary>
        /// Converts a <see cref="string"/> to an expression
        /// </summary>
        /// <param name="expr">
        /// <see cref="string"/> expression, for example, <code>"2 * x + 3 + sqrt(x)"</code>
        /// </param>
        /// <returns>
        /// The parsed expression
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity FromString(string expr) => Parser.Parse(expr);

        /// <summary>
        /// Translates a <see cref="Number"/> in base 10 into base <paramref name="N"/>
        /// </summary>
        /// <param name="num">
        /// A <see cref="RealNumber"/> in base 10 to be translated into base <paramref name="N"/>
        /// </param>
        /// <param name="N">
        /// The base to translate the number into
        /// </param>
        /// <returns>
        /// A <see cref="string"/> with the number in the required base
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToBaseN(RealNumber num, int N) => BaseConversion.ToBaseN(num.Value, N);

        /// <summary>
        /// Translates a number in base <paramref name="N"/> into base 10
        /// </summary>
        /// <param name="num">
        /// A <see cref="RealNumber"/> in base <paramref name="N"/> to be translated into base 10
        /// </param>
        /// <param name="N">
        /// The base to translate the number from
        /// </param>
        /// <returns>
        /// The <see cref="RealNumber"/> in base 10
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RealNumber FromBaseN(string num, int N) => BaseConversion.FromBaseN(num, N);

        /// <summary>
        /// Returns the <a href="https://en.wikipedia.org/wiki/LaTeX">LaTeX</a> representation of the argument
        /// </summary>
        /// <param name="latexiseable">
        /// Any element (<see cref="Entity"/>, <see cref="Set"/>, etc.) that can be represented in LaTeX
        /// </param>
        /// <returns></returns>
        public static string Latex(ILatexiseable latexiseable) => latexiseable.Latexise();

        /// <summary>
        /// <para>All operations for <see cref="Number"/> and its derived classes are available from here.</para>
        ///
        /// These methods represent the only possible way to explicitly create numeric instances.
        /// It will automatically downcast the result for you,
        /// so <code>Number.Create(1.0);</code> is an <see cref="IntegerNumber"/>.
        /// To avoid it, you may temporarily disable it
        /// <code>
        ///  MathS.Settings.DowncastingEnabled.As(false, () =>
        /// {
        ///    var yourNum = Number.Create(1.0);
        /// });
        /// </code>
        /// and the result will be a <see cref="RealNumber"/>.
        /// </summary>
        public static class Numbers
        {
            /// <summary>
            /// Creates an instance of <see cref="ComplexNumber"/> from <see cref="Complex"/>
            /// </summary>
            /// <param name="value">
            /// A value of type <see cref="Complex"/>
            /// </param>
            /// <returns>
            /// The resulting <see cref="ComplexNumber"/>
            /// </returns>
            public static ComplexNumber Create(Complex value) =>
                Create(EDecimal.FromDouble(value.Real), EDecimal.FromDouble(value.Imaginary));

            /// <summary>
            /// Creates an instance of <see cref="IntegerNumber"/> from a <see cref="long"/>
            /// </summary>
            /// <param name="value">
            /// A value of type <see cref="long"/> (signed 64-bit integer)
            /// </param>
            /// <returns>
            /// The resulting <see cref="IntegerNumber"/>
            /// </returns>
            public static IntegerNumber Create(long value) => IntegerNumber.Create(value);

            /// <summary>
            /// Creates an instance of <see cref="IntegerNumber"/> from an <see cref="EInteger"/>
            /// </summary>
            /// <param name="value">
            /// A value of type <see cref="EInteger"/>
            /// </param>
            /// <returns>
            /// The resulting <see cref="IntegerNumber"/>
            /// </returns>
            public static IntegerNumber Create(EInteger value) => IntegerNumber.Create(value);

            /// <summary>
            /// Creates an instance of <see cref="IntegerNumber"/> from an <see cref="int"/>
            /// </summary>
            /// <param name="value">
            /// A value of type <see cref="int"/> (signed 32-bit integer)
            /// </param>
            /// <returns>
            /// The resulting <see cref="IntegerNumber"/>
            /// </returns>
            public static IntegerNumber Create(int value) => IntegerNumber.Create(value);

            /// <summary>
            /// Creates an instance of <see cref="RationalNumber"/> from two <see cref="EInteger"/>s
            /// </summary>
            /// <param name="numerator">
            /// Numerator of type <see cref="EInteger"/>
            /// </param>
            /// <param name="denominator">
            /// Denominator of type <see cref="EInteger"/>
            /// </param>
            /// <returns>
            /// The resulting <see cref="RationalNumber"/>
            /// </returns>
            public static RationalNumber CreateRational(EInteger numerator, EInteger denominator)
                => RationalNumber.Create(numerator, denominator);

            /// <summary>
            /// Creates an instance of <see cref="RationalNumber"/> from an <see cref="ERational"/>
            /// </summary>
            /// <param name="rational">
            /// A value of type <see cref="ERational"/>
            /// </param>
            /// <returns>
            /// The resulting <see cref="RationalNumber"/>
            /// </returns>
            public static RationalNumber Create(ERational rational) => RationalNumber.Create(rational);

            /// <summary>
            /// Creates an instance of <see cref="RealNumber"/> from an <see cref="EDecimal"/>
            /// </summary>
            /// <param name="value">
            /// A value of type <see cref="EDecimal"/>
            /// </param>
            /// <returns>
            /// The resulting <see cref="RealNumber"/>
            /// </returns>
            public static RealNumber Create(EDecimal value) => RealNumber.Create(value);

            /// <summary>
            /// Creates an instance of <see cref="RealNumber"/> from a <see cref="double"/>
            /// </summary>
            /// <param name="value">
            /// A value of type <see cref="double"/> (64-bit floating-point number)
            /// </param>
            /// <returns>
            /// The resulting <see cref="RealNumber"/>
            /// </returns>
            public static RealNumber Create(double value) => RealNumber.Create(EDecimal.FromDouble(value));

            /// <summary>
            /// Creates an instance of <see cref="ComplexNumber"/> from two <see cref="RealNumber"/>s
            /// </summary>
            /// <param name="re">
            /// Real part of the desired <see cref="ComplexNumber"/> of type <see cref="EDecimal"/>
            /// </param>
            /// <param name="im">
            /// Imaginary part of the desired <see cref="ComplexNumber"/> of type <see cref="EDecimal"/>
            /// </param>
            /// <returns>
            /// The resulting <see cref="ComplexNumber"/>
            /// </returns>
            public static ComplexNumber Create(EDecimal re, EDecimal im) => ComplexNumber.Create(re, im);
        }

        /// <summary>
        /// Classes and functions related to matrices are defined here
        /// </summary>
        public static class Matrices
        {
            /// <summary>
            /// Creates an instance of <see cref="Tensor"/> that is a matrix.
            /// Usage example:
            /// <code>
            /// var t = MathS.Matrix(5, 3,
            ///     10, 11, 12,
            ///     20, 21, 22,
            ///     30, 31, 32,
            ///     40, 41, 42,
            ///     50, 51, 52);
            /// </code>
            /// creates 5×3 matrix with the appropriate elements
            /// </summary>
            /// <param name="rows">
            /// Number of rows (first axis)
            /// </param>
            /// <param name="columns">
            /// Number of columns (second axis)
            /// </param>
            /// <param name="values">
            /// Array of values of the matrix so that its length is equal to
            /// the product of <paramref name="rows"/> and <paramref name="columns"/>
            /// </param>
            /// <returns>
            /// A two-dimensional <see cref="Tensor"/> which is a matrix
            /// </returns>
            public static Tensor Matrix(int rows, int columns, params Entity[] values) =>
                new(GenTensor.CreateMatrix(rows, columns, (x, y) => values[x * columns + y]));

            /// <summary>
            /// Creates an instance of <see cref="Tensor"/> that is a matrix.
            /// </summary>
            /// <param name="values">
            /// A two-dimensional array of values
            /// </param>
            /// <returns>
            /// A two-dimensional <see cref="Tensor"/> which is a matrix
            /// </returns>
            public static Tensor Matrix(Entity[,] values) => new(GenTensor.CreateMatrix(values));

            /// <summary>
            /// Creates an instance of <see cref="Tensor"/> that is a vector.
            /// </summary>
            /// <param name="values">
            /// The cells of the <see cref="Tensor"/>
            /// </param>
            /// <returns>
            /// A one-dimensional <see cref="Tensor"/> which is a vector
            /// </returns>
            public static Tensor Vector(params Entity[] values) => new(GenTensor.CreateVector(values));

            /// <summary>
            /// Returns the dot product of two <see cref="Tensor"/>s that are matrices.
            /// </summary>
            /// <param name="A">
            /// First matrix (its width is the result's width)
            /// </param>
            /// <param name="B">
            /// Second matrix (its height is the result's height)
            /// </param>
            /// <returns>
            /// A two-dimensional <see cref="Tensor"/> (matrix) as a result of symbolic multiplication
            /// </returns>
            [Obsolete("Use MatrixMultiplication instead")]
            public static Tensor DotProduct(Tensor A, Tensor B) => new(GenTensor.MatrixMultiply(A.InnerTensor, B.InnerTensor));

            /// <summary>
            /// Returns the dot product of two <see cref="Tensor"/>s that are matrices.
            /// </summary>
            /// <param name="A">
            /// First matrix (its width is the result's width)
            /// </param>
            /// <param name="B">
            /// Second matrix (its height is the result's height)
            /// </param>
            /// <returns>
            /// A two-dimensional <see cref="Tensor"/> (matrix) as a result of symbolic multiplication
            /// </returns>
            public static Tensor MatrixMultiplication(Tensor A, Tensor B) => new(GenTensor.TensorMatrixMultiply(A.InnerTensor, B.InnerTensor));

            /// <summary>
            /// Returns the scalar product of two <see cref="Tensor"/>s that are vectors
            /// with the same length.
            /// </summary>
            /// <param name="a">
            /// First vector (order does not matter)
            /// </param>
            /// <param name="b">
            /// Second vector
            /// </param>
            /// <returns>
            /// The resulting scalar which is an <see cref="Entity"/> and not a <see cref="Tensor"/>
            /// </returns>
            public static Entity ScalarProduct(Tensor a, Tensor b) => GenTensor.VectorDotProduct(a.InnerTensor, b.InnerTensor);
        }

        /// <summary>
        /// A couple of settings allowing you to set some preferences for AM's algorithms
        /// To use these settings the syntax is
        /// <code>
        /// MathS.Settings.SomeSetting.As(5 /* Here you set a value to the setting */, () =>
        /// {
        ///     ... /* your code */
        /// });
        /// </code>
        /// </summary>
        public static partial class Settings
        {
            /// <summary>
            /// Enables downcasting. Not recommended to turn off, disabling might be only useful for some calculations
            /// </summary>
            public static Setting<bool> DowncastingEnabled => GetCurrentOrDefault(ref downcastingEnabled, true);

            /// <summary>
            /// Amount of iterations allowed for attempting to cast to a rational
            /// The more iterations, the larger fraction could be calculated
            /// </summary>
            public static Setting<int> FloatToRationalIterCount => GetCurrentOrDefault(ref floatToRationalIterCount, 15);

            /// <summary>
            /// If a numerator or denominator is too large, it's suspended to better keep the real number instead of casting
            /// </summary>
            public static Setting<EInteger> MaxAbsNumeratorOrDenominatorValue =>
                GetCurrentOrDefault(ref maxAbsNumeratorOrDenominatorValue, 100000000);

            /// <summary>
            /// Sets threshold for comparison
            /// For example, if you don't need precision higher than 6 digits after .,
            /// you can set it to 1.0e-6 so 1.0000000 == 0.9999999
            /// </summary>
            public static Setting<EDecimal> PrecisionErrorCommon =>
                GetCurrentOrDefault(ref precisionErrorCommon, 1e-6m);


            /// <summary>
            /// Numbers whose absolute value is less than PrecisionErrorZeroRange are considered zeros
            /// </summary>
            public static Setting<EDecimal> PrecisionErrorZeroRange => GetCurrentOrDefault(ref precisionErrorZeroRange, 1e-16m);

            /// <summary>
            /// If you only need analytical solutions and an empty set if no analytical solutions were found, disable Newton's method
            /// </summary>
            public static Setting<bool> AllowNewton => GetCurrentOrDefault(ref allowNewton, true);

            /// <summary>
            /// Criteria for simplifier so you could control which expressions are considered easier by you
            /// </summary>
            public static Setting<Func<Entity, int>> ComplexityCriteria =>
                GetCurrentOrDefault(ref complexityCriteria, expr =>
                {
                    var res = 0;

                    // Number of nodes
                    res += expr.Complexity;

                    // Number of variables
                    res += expr.Count(entity => entity is Variable);

                    // Number of divides
                    res += expr.Count(entity => entity is Divf) / 2;

                    // Number of negative powers
                    res += expr.Count(entity => entity is Powf(_, RealNumber { IsNegative: true }));

                    return res;
                });

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
            public static Setting<NewtonSetting> NewtonSolver => GetCurrentOrDefault(ref newtonSolver, new NewtonSetting());

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
            public static Setting<int> MaxExpansionTermCount => GetCurrentOrDefault(ref maxExpansionTermCount, 50);

            /// <summary>
            /// Settings for <see cref="EDecimal"/> precisions of <a href="https://github.com/peteroupc/Numbers">PeterO.Numbers</a>
            /// </summary>
            public static Setting<EContext> DecimalPrecisionContext =>
                GetCurrentOrDefault(ref decimalPrecisionContext, new EContext(100, ERounding.HalfUp, -100, 1000, false));
        }

        /// <summary>
        /// Some additional functions are defined here
        /// </summary>
        public static class Utils
        {
            /// <summary>
            /// Returns an <see cref="Entity"/> in polynomial order if possible
            /// </summary>
            /// <param name="expr">
            /// The unordered <see cref="Entity"/>
            /// </param>
            /// <param name="variable">
            /// The variable of the polynomial
            /// </param>
            /// <param name="dst">
            /// The ordered result
            /// </param>
            /// <returns>
            /// <see langword="true"/> if success,
            /// <see langword="false"/> otherwise
            /// (do not access <paramref name="dst"/> in this case, it's undefined)
            /// </returns>
            public static bool TryPolynomial(Entity expr, Variable variable,
                [System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
                out Entity? dst) => Functions.Utils.TryPolynomial(expr, variable, out dst);

            /// <summary>
            /// Optimizes <paramref name="tree"/> to a binary tree.
            /// This might boost some operations but is not necessary to use
            /// </summary>
            /// <param name="tree">
            /// An expression (tree) to optimize
            /// </param>
            /// <returns>
            /// An optimized but logically equal tree
            /// </returns>
            public static Entity OptimizeTree(Entity tree) => tree.Replace(Patterns.OptimizeRules);

            /// <summary>
            /// Returns sympy interpretable format
            /// </summary>
            /// <param name="expr">
            /// An <see cref="Entity"/> representing an expression
            /// </param>
            /// <returns></returns>
            public static string ToSympyCode(Entity expr)
            {
                var sb = new System.Text.StringBuilder();
                var vars = expr.Vars;
                sb.Append("import sympy\n\n");
                foreach (var f in vars)
                    sb.Append($"{f} = sympy.Symbol('{f}')\n");
                sb.Append("\n");
                sb.Append("expr = " + expr.ToSymPy());
                return sb.ToString();
            }
        }

        /// <summary>
        /// Functions and classes related to sets defined here
        /// 
        /// Class <see cref="SetNode"/> defines true mathematical sets
        /// It can be empty,
        /// it can contain <see cref="OneElementPiece"/>s,
        /// it can contain <see cref="IntervalPiece"/>s etc.
        /// It supports intersection (with & operator), union (with | operator),
        ///             subtraction (with - operator) as well as inversion (with ! operator).
        /// </summary>
        public static class Sets
        {
            /// <summary>
            /// Creates an instance of an empty <see cref="Set"/>
            /// </summary>
            /// <returns>
            /// A <see cref="Set"/> with no elements
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Set Empty()
                => new Set();

            /// <returns>A set of all <see cref="ComplexNumber"/>s</returns>
            public static Set C()
                => Set.C();

            /// <returns>A set of all <see cref="RealNumber"/>s</returns>
            public static Set R()
                => Set.R();

            /// <summary>
            /// Creats a <see cref="Set"/> that you can fill with elements
            /// Later on, you may add an Interval if you wish
            /// </summary>
            public static Set Finite(params Entity[] entities)
                => Set.Finite(entities);

            /// <summary>
            /// Creates an interval. To modify it, use e.g.
            /// <see cref="IntervalPiece.SetLeftClosed(bool)"/>
            /// (see more alike functions in <see cref="Set"/> documentation)
            /// </summary>
            public static IntervalPiece Interval(Entity from, Entity to) => Piece.Interval(from, to);

            /// <summary>
            /// Creates an element for <see cref="Set"/>. One can be created implicitly, <code>Piece a = 3;</code>
            /// </summary>
            public static OneElementPiece Element(Entity element)
                => new OneElementPiece(element);
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

            /// <summary>
            /// Derives over <paramref name="x"/> <paramref name="power"/> times
            /// </summary>
            public static Entity? Derivative(Entity expr, Variable x, EInteger power)
            {
                var ent = expr;
                for (var _ = 0; _ < power; _++)
                    ent = ent is { } ? Derivative(ent, x) : ent;
                return ent;
            }

            /// <summary>
            /// Derivation over a variable (without simplification)
            /// </summary>
            /// <param name="x">
            /// The variable to derive over
            /// </param>
            /// <returns>The derived result</returns>
            public static Entity? Derivative(Entity expr, Variable x) => expr.Derive(x);

            /// <summary>
            /// Integrates over <paramref name="x"/> <paramref name="power"/> times
            /// </summary>
            public static Entity? Integral(Entity expr, Variable x, EInteger power)
            {
                var ent = expr;
                for (var _ = 0; _ < power; _++)
                    ent = ent is { } ? Integral(ent, x) : ent;
                return ent;
            }

            /// <summary>
            /// Integrates over a variable (without simplification)
            /// </summary>
            /// <param name="x">
            /// The variable to integrate over
            /// </param>
            /// <returns>The integrated result</returns>
            public static Entity? Integral(Entity expr, Variable x) =>
                throw new NotImplementedException("Integrals not implemented yet");
        }
        /// <summary>
        /// Hangs your <see cref="Entity"/> to a derivative node
        /// (to evaluate instead use <see cref="Compute.Derivative(Entity, Variable)"/>)
        /// </summary>
        /// <param name="expr">
        /// Expression to be hung
        /// </param>
        /// <param name="var">
        /// Variable over which derivative is taken
        /// </param>
        public static Entity Derivative(Entity expr, Entity var)
            => new Derivativef(expr, var, 1);

        /// <summary>
        /// Hangs your <see cref="Entity"/> to a derivative node
        /// (to evaluate instead use <see cref="Compute.Derivative(Entity, Variable)"/>)
        /// </summary>
        /// <param name="expr">
        /// Expression to be hung
        /// </param>
        /// <param name="var">
        /// Variable over which derivative is taken
        /// </param>
        /// <param name="power">
        /// Number of times derivative is taken
        /// Only integers will be simplified or evaluated
        /// </param>
        public static Entity Derivative(Entity expr, Entity var, Entity power)
            => new Derivativef(expr, var, power);

        /// <summary>
        /// Hangs your entity to an integral node
        /// (to evaluate instead use <see cref="Compute.Integral(Entity, Variable)"/>)
        /// </summary>
        /// <param name="expr">
        /// Expression to be hung
        /// </param>
        /// <param name="var">
        /// Variable over which integral is taken
        /// </param>
        public static Entity Integral(Entity expr, Entity var)
            => new Integralf(expr, var, 1);

        /// <summary>
        /// Hangs your entity to an integral node
        /// (to evaluate instead use <see cref="Compute.Integral(Entity, Variable)"/>)
        /// </summary>
        /// <param name="expr">
        /// Expression to be hung
        /// </param>
        /// <param name="var">
        /// Variable over which integral is taken
        /// </param>
        /// <param name="power">
        /// Number of times integral is taken
        /// Only integers will be simplified or evaluated
        /// </param>
        public static Entity Integral(Entity expr, Entity var, Entity power)
            => new Integralf(expr, var, power);

        /// <summary>
        /// Hangs your entity to a limit node
        /// (to evaluate instead use <see cref="Compute.Limit(Entity, Variable, Entity)"/>)
        /// </summary>
        /// <param name="expr">
        /// Expression to be hung
        /// </param>
        /// <param name="var">
        /// Variable over which limit is taken
        /// </param>
        /// <param name="dest">
        /// Where <paramref name="var"/> approaches (could be finite or infinite)
        /// </param>
        /// <param name="approach">
        /// From where it approaches
        /// </param>
        public static Entity Limit(Entity expr, Entity var, Entity dest, ApproachFrom approach = ApproachFrom.BothSides)
            => new Limitf(expr, var, dest, approach);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles",
            Justification = "Lowercase constants as written in Mathematics")]
        public static class DecimalConst
        {
            /// <summary><a href="https://en.wikipedia.org/wiki/Pi"/></summary>
            public static EDecimal pi =>
                PeterONumbersExtensions.ConstantCache.Lookup(Settings.DecimalPrecisionContext).Pi;

            /// <summary><a href="https://en.wikipedia.org/wiki/E_(mathematical_constant)"/></summary>
            public static EDecimal e =>
                PeterONumbersExtensions.ConstantCache.Lookup(Settings.DecimalPrecisionContext).E;
        }
    }
}
