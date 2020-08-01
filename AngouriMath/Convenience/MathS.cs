
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
using System.Numerics;
using AngouriMath.Core;
using AngouriMath.Core.FromString;
using AngouriMath.Core.TreeAnalysis;
using AngouriMath.Functions.NumberSystem;
using AngouriMath.Functions.Output;
using AngouriMath.Functions.Algebra.Solver;
using System.Runtime.CompilerServices;
using AngouriMath.Core.Numerix;
using AngouriMath.Core.Sys;
 using AngouriMath.Core.Sys.Interfaces;
using AngouriMath.Core.Sys.Items.Tensors;
using AngouriMath.Functions;
using AngouriMath.Functions.Algebra.AnalyticalSolving;
using AngouriMath.Functions.Algebra.InequalitySolver;
using AngouriMath.Functions.DiscreteMath;
using GenericTensor.Core;
using Number = AngouriMath.Core.Numerix.Number;
using PeterO.Numbers;

namespace AngouriMath
{
    /// <summary>
    /// Use functions from this class
    /// </summary>
    public static partial class MathS
    {
        /// <summary>
        /// Use it to solve equations
        /// </summary>
        /// <param name="equations">
        /// An array of Entity (or strings)
        /// the system consists of
        /// </param>
        /// <returns>
        /// A type of <see cref="EquationSystem"/> which can be then solved
        /// </returns>
        public static EquationSystem Equations(params Entity[] equations)
        => new EquationSystem(equations);


        /// <summary>
        /// Solves one equation over one variable
        /// </summary>
        /// <param name="equation">
        /// An equation that is assumed to be 0
        /// </param>
        /// <param name="var">
        /// Variable whose values we are looking for
        /// </param>
        /// <returns>
        /// Returns a <see cref="Set"/> of possible values or intervals of values
        /// </returns>
        public static Set SolveEquation(Entity equation, VariableEntity var)
            => EquationSolver.Solve(equation, var);

        /// <summary>
        /// Will be added soon!
        /// Solves an inequality numerically
        /// </summary>
        /// <param name="inequality">
        /// This must only contain one variable, which is var
        /// </param>
        /// <param name="var">
        /// The only variable
        /// </param>
        /// <param name="sign">
        /// ">", "<", ">=", "<="
        /// </param>
        /// <returns></returns>
        public static Set SolveInequalityNumerically(Entity inequality, VariableEntity var, string sign)
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
        /// Argument-node of sine
        /// </param>
        /// <returns>
        /// Sine-node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Sin(Entity a) => Sinf.Hang(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Trigonometric_functions"/>
        /// </summary>
        /// <param name="a">
        /// Argument-node of cosine
        /// </param>
        /// <returns>
        /// Cosine-node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Cos(Entity a) => Cosf.Hang(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Logarithm"/>
        /// </summary>
        /// <param name="@base">
        /// base-node of logarithm
        /// </param>
        /// <param name="x">
        /// Argument-node of logarithm
        /// </param>
        /// <returns>
        /// Logarithm-node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Log(Entity @base, Entity x) => Logf.Hang(@base, x);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Power_function"/>
        /// </summary>
        /// <param name="base">
        /// Base-node of power
        /// </param>
        /// <param name="power">
        /// Power-node 
        /// </param>
        /// <returns>
        /// Exponential node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Pow(Entity @base, Entity power) => Powf.Hang(@base, power);

        /// <summary>
        /// Special case of <a href="https://en.wikipedia.org/wiki/Power_function"/> function
        /// </summary>
        /// <param name="a">
        /// The argument of which square root will be taken
        /// </param>
        /// <returns>
        /// a ^ (1 / 2) node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Sqrt(Entity a) => Powf.Hang(a, RationalNumber.Create(1, 2));

        /// <summary>
        /// Special case of <a href="https://en.wikipedia.org/wiki/Power_function"/> function
        /// </summary>
        /// <param name="a">
        /// The argument of which cubic root will be taken
        /// </param>
        /// <returns>
        /// a ^ (1 / 3) node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Cbrt(Entity a) => Powf.Hang(a, RationalNumber.Create(1, 3));

        /// <summary>
        /// Special case of <a href="https://en.wikipedia.org/wiki/Power_function"/> function
        /// </summary>
        /// <param name="a">
        /// Argument to be powered to 2
        /// </param>
        /// <returns>
        /// a ^ 2
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Sqr(Entity a) => Powf.Hang(a, 2);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Trigonometric_functions"/>
        /// </summary>
        /// <param name="a">
        /// Argument node of which Tangen will be taken
        /// </param>
        /// <returns>
        /// Tangent node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Tan(Entity a) => Tanf.Hang(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Trigonometric_functions"/>
        /// </summary>
        /// <param name="a">
        /// Argument node of which Cotangen will be taken
        /// </param>
        /// <returns>
        /// Cotangent node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Cotan(Entity a) => Cotanf.Hang(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Trigonometric_functions"/>
        /// </summary>
        /// <param name="a">
        /// Argument node of which Secant will be taken
        /// </param>
        /// <returns>
        /// Secant node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Sec(Entity a) => 1 / Cos(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Trigonometric_functions"/>
        /// </summary>
        /// <param name="a">
        /// Argument node of which Cosecant will be taken
        /// </param>
        /// <returns>
        /// Cosecant node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Cosec(Entity a) => 1 / Sin(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Inverse_trigonometric_functions"/>
        /// </summary>
        /// <param name="a">
        /// Argument node of which Arcsine will be taken
        /// </param>
        /// <returns>
        /// Arcsine node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Arcsin(Entity a) => Arcsinf.Hang(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Inverse_trigonometric_functions"/>
        /// </summary>
        /// <param name="a">
        /// Argument node of which Arccosine will be taken
        /// </param>
        /// <returns>
        /// Arccosine node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Arccos(Entity a) => Arccosf.Hang(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Inverse_trigonometric_functions"/>
        /// </summary>
        /// <param name="a">
        /// Argument node of which Arctangent will be taken
        /// </param>
        /// <returns>
        /// Arctangent node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Arctan(Entity a) => Arctanf.Hang(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Inverse_trigonometric_functions"/>
        /// </summary>
        /// <param name="a">
        /// Argument node of which Arccotangent will be taken
        /// </param>
        /// <returns>
        /// Arccotangent node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Arccotan(Entity a) => Arccotanf.Hang(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Inverse_trigonometric_functions"/>
        /// </summary>
        /// <param name="a">
        /// Argument node of which Arcsecant will be taken
        /// </param>
        /// <returns>
        /// Arcsecant node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Arcsec(Entity a) => Arccosf.Hang(1 / a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Inverse_trigonometric_functions"/>
        /// </summary>
        /// <param name="a">
        /// Argument node of which Arccosecant will be taken
        /// </param>
        /// <returns>
        /// Arccosecant node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Arccosec(Entity a) => Arcsinf.Hang(1 / a);

        /// <summary>
        /// Is a special case of logarithm with the base equal
        /// <a href="https://en.wikipedia.org/wiki/E_(mathematical_constant)">e</a>:
        /// <a href="https://en.wikipedia.org/wiki/Natural_logarithm"/>
        /// </summary>
        /// <param name="a">
        /// Argument node of which Natural logarithm will be taken
        /// </param>
        /// <returns>
        /// Tangent node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Ln(Entity a) => Logf.Hang(e, a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Factorial"/>
        /// </summary>
        /// <param name="a">
        /// Argument node of which factorial will be taken
        /// </param>
        /// <returns>
        /// Tangent node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Factorial(Entity a) => Factorialf.Hang(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Gamma_function"/>
        /// </summary>
        /// <param name="a">
        /// Argument node of which Gamma-function will be taken
        /// </param>
        /// <returns>
        /// Gamma node
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Gamma(Entity a) => Factorialf.Hang(a + 1);

        /// <summary>
        /// Creates an instance of variable entity.
        /// </summary>
        /// <param name="name">
        /// Its name set with this parameter
        /// Same names will lead to variables counted as same
        /// </param>
        /// <returns>
        /// Tangent node
        /// </returns>
        public static VariableEntity Var(string name) => new VariableEntity(name);

        /// <summary>
        /// Creates a complex instance of Number (not NumberEntity!)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete("Use Number.Create or implicit construction instead")]
        public static Number Num(EDecimal a, EDecimal b) => ComplexNumber.Create(a, b);

        /// <summary>
        /// Creates a real instance of Number (not NumberEntity!)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete("Use Number.Create or implicit construction instead")]
        public static ComplexNumber Num(EDecimal a) => RealNumber.Create(a);

        // List of public constants
        // ReSharper disable once InconsistentNaming
        public static readonly VariableEntity e = "e";
        // ReSharper disable once InconsistentNaming
        public static readonly ComplexNumber i = ComplexNumber.ImaginaryOne;
        // ReSharper disable once InconsistentNaming
        public static readonly VariableEntity pi = "pi";

        /// <summary>
        /// Converts an expression from a string
        /// </summary>
        /// <param name="expr">
        /// String expression, for example, "2 * x + 3 + sqrt(x)"
        /// </param>
        /// <returns>
        /// Returns a parsed expression
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity FromString(string expr)
            => Parser.Parse(expr);

        /// <summary>
        /// Translates num10 into another number system
        /// </summary>
        /// <param name="num">
        /// A real (floating) number in decimal format to be
        /// translated into base N
        /// </param>
        /// <param name="N">
        /// Base that number to be translated into
        /// </param>
        /// <returns>
        /// A string with the number in the required number base
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToBaseN(RealNumber num, int N) 
            => NumberSystem.ToBaseN(num.Value, N);

        /// <summary>
        /// Translates num into 10 number system
        /// </summary>
        /// <param name="num">
        /// A real (floating) number in base N
        /// to be translated into decimal format
        /// </param>
        /// <param name="N">
        /// Base from which this number is going to be translated
        /// </param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RealNumber FromBaseN(string num, int N)
            => NumberSystem.FromBaseN(num, N);

        /// <summary>
        /// Returns <a href="https://en.wikipedia.org/wiki/LaTeX">LaTeX</a> code of the argument
        /// </summary>
        /// <param name="latexiseable">
        /// Any element (Entity, Set, etc.) that can be
        /// translated into LaTeX format
        /// </param>
        /// <returns></returns>
        public static string Latex(ILatexiseable latexiseable)
            => latexiseable.Latexise();

        /// <summary>
        /// All operations for Number and its derived classes are available from here
        ///
        /// This list represents the only possible way to explicitly create numeric instances
        /// It will automatically downcast the result for you, so that 1.0 is an IntegerNumber
        /// To avoid it, you may temporarily disable it
        /// <code>
        ///  MathS.Settings.DowncastingEnabled.As(false, () =>
        /// {
        ///    var yourNum = Number.Create(1.0);
        /// });
        /// </code>
        /// </summary>
        public static class Numbers
        {
            /// <summary>
            /// Creates an instance of ComplexNumber from System.Numerics.Complex
            /// </summary>
            /// <param name="value">
            /// value of <see cref="System.Numerics.Complex"/> type
            /// </param>
            /// <returns>
            /// ComplexNumber
            /// </returns>
            public static ComplexNumber Create(Complex value) =>
                Create(EDecimal.FromDouble(value.Real), EDecimal.FromDouble(value.Imaginary));

            /// <summary>
            /// Creates an instance of IntegerNumber from long
            /// </summary>
            /// <param name="value">
            /// value of long (signed 64-bit integer) type
            /// </param>
            /// <returns>
            /// IntegerNumber
            /// </returns>
            public static IntegerNumber Create(long value) => IntegerNumber.Create(value);

            /// <summary>
            /// Creates an instance of IntegerNumber from System.Numerics.EInteger
            /// </summary>
            /// <param name="value">
            /// value of <see cref="EInteger"/> type
            /// </param>
            /// <returns>
            /// IntegerNumber
            /// </returns>
            public static IntegerNumber Create(EInteger value) => IntegerNumber.Create(value);

            /// <summary>
            /// Creates an instance of IntegerNumber from int
            /// </summary>
            /// <param name="value">
            /// value of int (signed 32-bit integer) type
            /// </param>
            /// <returns>
            /// IntegerNumber
            /// </returns>
            public static IntegerNumber Create(int value) => IntegerNumber.Create(value);

            /// <summary>
            /// Creates an instance of RationalNumber of two EInteger
            /// </summary>
            /// <param name="numerator">
            /// Numerator of type <see cref="EInteger"/>
            /// </param>
            /// <param name="denominator">
            /// Denominator of type <see cref="EInteger"/>
            /// </param>
            /// <returns>
            /// RationalNumber
            /// </returns>
            public static RationalNumber CreateRational(EInteger numerator, EInteger denominator)
                => RationalNumber.Create(numerator, denominator);

            /// <summary>
            /// Creates an instance of RationalNumber of two IntegerNumbers
            /// </summary>
            /// <param name="rational">
            /// Value of type <see cref="ERational"/>
            /// </param>
            /// <returns>
            /// RationalNumber
            /// </returns>
            public static RationalNumber Create(ERational rational) => RationalNumber.Create(rational);

            /// <summary>
            /// Creates an instance of RealNumber from EDecimal
            /// </summary>
            /// <param name="value">
            /// value of type <see cref="EDecimal"/>
            /// </param>
            /// <returns>
            /// RealNumber
            /// </returns>
            public static RealNumber Create(EDecimal value) => RealNumber.Create(value);

            /// <summary>
            /// Creates an instance of RealNumber from double
            /// </summary>
            /// <param name="value">
            /// value of type double (64-bit floating type)
            /// </param>
            /// <returns>
            /// RealNumber
            /// </returns>
            public static RealNumber Create(double value) => RealNumber.Create(EDecimal.FromDouble(value));

            /// <summary>
            /// Creates an instance of ComplexNumber from two RealNumbers
            /// </summary>
            /// <param name="re">
            /// Real part of a desired complex number of type <see cref="EDecimal"/>
            /// </param>
            /// <param name="im">
            /// Imaginary part of a desired complex number of type <see cref="EDecimal"/>
            /// </param>
            /// <returns>
            /// ComplexNumber
            /// </returns>
            public static ComplexNumber Create(EDecimal re, EDecimal im) => ComplexNumber.Create(re, im);
        }

        /// <summary>
        /// Classes and functions related to matrices defined here
        /// </summary>
        public static class Matrices
        {
            /// <summary>
            /// Creates an instance of Tensor: Matrix
            /// Usage example:
            /// var t = MathS.Matrix(5, 3,
            ///        10, 11, 12,
            ///        20, 21, 22,
            ///        30, 31, 32,
            ///        40, 41, 42,
            ///        50, 51, 52
            ///        );
            /// creates matrix 5x3 with the appropriate elements
            /// </summary>
            /// <param name="rows">
            /// Number of rows (first axis)
            /// </param>
            /// <param name="columns">
            /// Numbers of columns (second axis)
            /// </param>
            /// <param name="values">
            /// Array of values of the matrix so that its length is equal to
            /// product of rows and columns
            /// </param>
            /// <returns>
            /// Two-dimensional Tensor
            /// </returns>
            public static Tensor Matrix(int rows, int columns, params Entity[] values)
                => TensorFunctional.Matrix(rows, columns, values);

            /// <summary>
            /// Creates an instance of Matrix
            /// </summary>
            /// <param name="values">
            /// 2-D array of values
            /// </param>
            /// <returns>
            /// Two-dimensional Tensor
            /// </returns>
            public static Tensor Matrix(Entity[,] values)
                => TensorFunctional.Matrix(values);

            /// <summary>
            /// Creates an instance of vector
            /// </summary>
            /// <param name="values">
            /// Cells of the tensor
            /// </param>
            /// <returns>
            /// One-dimensional tensor
            /// </returns>
            public static Tensor Vector(params Entity[] values)
                => TensorFunctional.Vector(values);

            /// <summary>
            /// Returns dot product of two matrices
            /// </summary>
            /// <param name="A">
            /// First matrix (its width is result's width)
            /// </param>
            /// <param name="B">
            /// Second matrix (its height is result's height)
            /// </param>
            /// <returns>
            /// Two-dimensional Tensor (matrix) as a result of symbolic multiplication
            /// </returns>
            [Obsolete("Use MatrixMultiplication instead")]
            public static Tensor DotProduct(Tensor A, Tensor B) =>
                TensorFunctional.DotProduct(A, B);

            /// <summary>
            /// Returns dot product of two matrices
            /// </summary>
            /// <param name="A">
            /// First matrix (its width is result's width)
            /// </param>
            /// <param name="B">
            /// Second matrix (its height is result's height)
            /// </param>
            /// <returns>
            /// Two-dimensional Tensor (matrix) as a result of symbolic multiplication
            /// </returns>
            public static Tensor MatrixMultiplication(Tensor A, Tensor B) =>
                new Tensor(GenTensor<Entity>.TensorMatrixMultiply(A.innerTensor, B.innerTensor));

            /// <summary>
            /// Returns scalar product of two vectors
            /// whose length should be the same
            /// </summary>
            /// <param name="A">
            /// First vector (order does not matter)
            /// </param>
            /// <param name="B">
            /// Second vector
            /// </param>
            /// <returns>
            /// Scalar (one Entity, not vector)
            /// </returns>
            public static Entity ScalarProduct(Tensor A, Tensor B) =>
                TensorFunctional.ScalarProduct(A, B);

        }

        /// <summary>
        /// A couple of settings allowing you to set some preferences for AM's algorithms
        /// To use those settings the syntax is
        /// MathS.Settings.SomeSetting.As(5, () =>  // Here you set a value to the setting
        /// {
        ///     ... // your code
        /// });
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
            public static Setting<Func<Entity, int>> ComplexityCriteria => GetCurrentOrDefault(ref complexityCriteria, Const.DefaultComplexityCriteria);

            /// <summary>
            /// Settings for the Newton-Raphson's root-search method
            /// e. g.
            /// MathS.Settings.NewtonSolver.As(
            /// new NewtonSetting() {
            ///     From = (-10, -10),
            ///     To = (10, 10),
            ///     StepCount = (10, 10),
            ///     Precision = 30
            /// }
            /// ...
            /// </summary>
            public static Setting<NewtonSetting> NewtonSolver => GetCurrentOrDefault(ref newtonSolver, new NewtonSetting());
            
            /// <summary>
            /// The maximum number of linear children of an expression in polynomial solver
            /// considering that there's no more than 1 children with the required variable, e. g.
            /// complexities are counted like that:
            /// (x + 2) ^ 2           -> 3 [x2, 4x, 4]
            /// x + 3 + a             -> 2 [x, 3 + a]
            /// (x + a)(b + c)        -> 2 [(b + c)x, a(b + c)]
            /// (x + 3 + a) / (x + 3) -> 2 [x / (x + 3), (3 + a) / (x + 3)]
            /// x2 + x + 1            -> 3 [x2, x, 1]
            /// </summary>
            public static Setting<int> MaxExpansionTermCount => GetCurrentOrDefault(ref maxExpansionTermCount, 50);

            /// <summary>
            /// Settings for EDecimal precisions of PeterO Numbers (https://github.com/peteroupc/Numbers)
            /// </summary>
            public static Setting<EContext> DecimalPrecisionContext =>
                GetCurrentOrDefault(ref decimalPrecisionContext,  new EContext(100, ERounding.HalfUp, -100, 1000, false));
        }

        /// <summary>
        /// Some additional functions defined here
        /// </summary>
        public static class Utils
        {
            /// <summary>
            /// Returns an entity in polynomial order if possible
            /// </summary>
            /// <param name="expr">
            /// To parse from
            /// </param>
            /// <param name="variable">
            /// Polynomial is a function of a variable
            /// </param>
            /// <param name="dst">
            /// To return to
            /// </param>
            /// <returns>
            /// <see langword="true"/> if success,
            /// <see langword="false"/> otherwise
            /// (do not access <paramref name="dst"/> in this case, it's undefined)
            /// </returns>
            public static bool TryPolynomial(Entity expr, VariableEntity variable,
                [System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
                out Entity? dst)
                => Functions.Utils.TryPolynomial(expr, variable, out dst);

            /// <summary>
            /// Checks tree for some unexpected bad occasions
            /// Throws <see cref="Core.Exceptions.SysException"/>'s children
            /// If you need a message, it's better to write
            /// <code>
            /// try
            /// {
            ///     MathS.CheckTree(a);
            /// }
            /// catch (SysException e)
            /// {
            ///     Console.WriteLine(e.Message);
            /// }
            /// </code>
            /// </summary>
            public static void CheckTree(Entity expr) 
                => TreeAnalyzer.CheckTree(expr);

            /// <summary>
            /// Optimizes tree to binary
            /// Might boost some operations
            /// Not necessary to use
            /// </summary>
            /// <param name="tree">
            /// An expression (tree) to optimize
            /// </param>
            /// <returns>
            /// An optimized but logically equal tree
            /// </returns>
            public static Entity OptimizeTree(Entity tree)
                => TreeAnalyzer.Optimization.OptimizeTree(tree);

            /// <summary>
            /// Returns sympy interpretable format
            /// </summary>
            /// <param name="expr">
            /// <see cref="Entity"/>-expression
            /// </param>
            /// <returns></returns>
            public static string ToSympyCode(Entity expr)
                => ToSympy.GenerateCode(expr);

            /// <summary>
            /// Returns list of unique variables, for example 
            /// it extracts `x`, `goose` from (x + 2 * goose) - pi * x
            /// </summary>
            /// <param name="expr">
            /// An expression to extract variables from
            /// </param>
            /// <returns>
            /// Set of unique variables excluding mathematical ones (such as pi, e...)
            /// </returns>
            public static Set GetUniqueVariables(Entity expr)
                => TreeAnalyzer.GetUniqueVariables(expr);

            /// <summary>
            /// Counts all nodes & subnodes that match criteria
            /// </summary>
            /// <param name="expr">
            /// Expr whose all subnodes will be processed
            /// </param>
            /// <param name="criteria">
            /// A condition a subtree should match to be counted
            /// </param>
            /// <returns>
            /// Number of hits
            /// </returns>
            public static int Count(Entity expr, Predicate<Entity> criteria)
                => TreeAnalyzer.Count(expr, criteria);
        }

        /// <summary>
        /// Functions and classes related to sets defined here
        /// 
        /// Class SetNode defines true mathematical sets
        /// It can be empty, it can contain numbers, it can contain intervals etc.
        /// It supports intersection (with & operator), union (with | operator), subtracting (with - operator)
        /// </summary>
        public static class Sets
        {
            /// <summary>
            /// Creates an instance of an empty set
            /// </summary>
            /// <returns>
            /// A <see cref="Set"/> with no elements
            /// </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static Set Empty()
                => new Set();

            /// <returns>A set of all complex numbers</returns>
            public static Set C()
                => Set.C();

            /// <returns>A set of all real numbers</returns>
            public static Set R()
                => Set.R();

            /// <summary>
            /// Creats a set that you can fill with elements
            /// Later on, you may add an Interval if you wish
            /// </summary>
            public static Set Finite(params Entity[] entities)
                => Set.Finite(entities);

            /// <summary>
            /// Creates an interval. To modify it, use e.g.
            /// <see cref="IntervalPiece.SetLeftClosed(bool)"/> (see more alike functions in set documentation)
            /// </summary>
            public static IntervalPiece Interval(Entity from, Entity to) => Piece.Interval(from, to);

            /// <summary>
            /// Creates an element for set. One can be created implicitly, <code>Piece a = 3;</code>
            /// </summary>
            public static OneElementPiece Element(Entity element)
                => new OneElementPiece(element);
        }

        /// <summary>
        /// Hangs your entity to a derivative node
        /// (to evaluate instead use <see cref="Entity.Derive(VariableEntity)">Derive</see>)
        /// </summary>
        /// <param name="expr">
        /// Expresesion to be hung
        /// </param>
        /// <param name="var">
        /// Variable over which derivative is taken
        /// </param>
        public static Entity Derivative(Entity expr, Entity var)
            => Derivativef.Hang(expr, var, 1);

        /// <summary>
        /// Hangs your entity to a derivative node
        /// (to evaluate instead use <see cref="Entity.Derive(VariableEntity, EInteger)">Derive</see>)
        /// </summary>
        /// <param name="expr">
        /// Expresesion to be hung
        /// </param>
        /// <param name="var">
        /// Variable over which derivative is taken
        /// </param>
        /// <param name="power">
        /// Number of times derivative is taken
        /// Other than integers will not be simplified or evaluated
        /// </param>
        public static Entity Derivative(Entity expr, Entity var, Entity power)
            => Derivativef.Hang(expr, var, power);

        /// <summary>
        /// Hangs your entity to an integral node
        /// (to evaluate instead use <see cref="Entity.Integrate(VariableEntity)">Derive</see>)
        /// </summary>
        /// <param name="expr">
        /// Expresesion to be hung
        /// </param>
        /// <param name="var">
        /// Variable over which derivative is taken
        /// </param>
        public static Entity Integral(Entity expr, Entity var)
            => Integralf.Hang(expr, var, 1);

        /// <summary>
        /// Hangs your entity to an integral node
        /// (to evaluate instead use <see cref="Entity.Integrate(VariableEntity, EInteger)">Derive</see>)
        /// </summary>
        /// <param name="expr">
        /// Expresesion to be hung
        /// </param>
        /// <param name="var">
        /// Variable over which derivative is taken
        /// </param>
        /// <param name="power">
        /// Number of times integral is taken
        /// Other than integers will not be simplified or evaluated
        /// </param>
        public static Entity Integral(Entity expr, Entity var, Entity power)
            => Integralf.Hang(expr, var, power);

        /// <summary>
        /// Hangs your entity to a limit node
        /// (to evaluate instead use <see cref="Entity.Limit(VariableEntity, Entity)">Derive</see>)
        /// </summary>
        /// <param name="expr">
        /// Expresesion to be hung
        /// </param>
        /// <param name="var">
        /// Variable over which derivative is taken
        /// </param>
        /// <param name="dest">
        /// Where var approaches (could be finite or infinite)
        /// </param>
        public static Entity Limit(Entity expr, Entity var, Entity dest)
            => Limitf.Hang(expr, var, dest, 0);

        /// <summary>
        /// Hangs your entity to a limit node
        /// (to evaluate instead use <see cref="Entity.Limit(VariableEntity, Entity, ApproachFrom)">Derive</see>)
        /// </summary>
        /// <param name="expr">
        /// Expresesion to be hung
        /// </param>
        /// <param name="var">
        /// Variable over which derivative is taken
        /// </param>
        /// <param name="dest">
        /// Where var approaches (could be finite or infinite)
        /// </param>
        /// <param name="approach">
        /// From where it approaches (left or right)
        /// </param>
        public static Entity Limit(Entity expr, Entity var, Entity dest, int approach)
            => Limitf.Hang(expr, var, dest, approach); // FromRight +1, Any 0, FromLeft -1 TODO: 1.1.0.4 limits

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles",
            Justification = "Lowercase constants as written in Mathamatics")]
        public static class DecimalConst
        {
            /// <summary>Pi constant</summary>
            public static EDecimal pi =>
                PeterONumbersExtensions.ConstantCache.Lookup(Settings.DecimalPrecisionContext).Pi;

            /// <summary>E constant</summary>
            public static EDecimal e =>
                PeterONumbersExtensions.ConstantCache.Lookup(Settings.DecimalPrecisionContext).E;
        }
    }
}
