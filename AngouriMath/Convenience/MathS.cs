
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
        /// <param name="equations"></param>
        /// <returns></returns>
        public static EquationSystem Equations(params Entity[] equations)
        => new EquationSystem(equations);


        /// <summary>
        /// Solves one equation over one variable
        /// </summary>
        /// <param name="equation"></param>
        /// <param name="var"></param>
        /// <returns></returns>
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
            return NumericalInequalitySolver.Solve(inequality, var, sign);
        }

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Trigonometric_functions"/>
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// A / C
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Sin(Entity a) => Sinf.Hang(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Trigonometric_functions"/>
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// B / C
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Cos(Entity a) => Cosf.Hang(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Logarithm"/>
        /// </summary>
        /// <param name="@base"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Log(Entity @base, Entity x) => Logf.Hang(@base, x);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Power_function"/>
        /// </summary>
        /// <param name="base_"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Pow(Entity base_, Entity power) => Powf.Hang(base_, power);

        /// <summary>
        /// Special case of power function
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// a ^ 0.5
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Sqrt(Entity a) => Powf.Hang(a, RationalNumber.Create(1, 2));

        /// <summary>
        /// Special case of power function
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// a ^ 2
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Sqr(Entity a) => Powf.Hang(a, 2);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Trigonometric_functions"/>
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// A / B
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Tan(Entity a) => Tanf.Hang(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Trigonometric_functions"/>
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// B / A
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Cotan(Entity a) => Cotanf.Hang(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Trigonometric_functions"/>
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// C / B
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Sec(Entity a) => 1 / Cos(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Trigonometric_functions"/>
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// C / A
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Cosec(Entity a) => 1 / Sin(a);

        /// <summary>
        /// This function is very interesting for ASC (<a href="https://asc-community.org"/>)
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// x * sin(x)
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity B(Entity a) => a * Sin(a);

        /// <summary>
        /// This function is very interesting for ASC (<a href="https://asc-community.org"/>)
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// x * cos(x)
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity TB(Entity a) => a * Cos(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Inverse_trigonometric_functions"/>
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// angle between A and C
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Arcsin(Entity a) => Arcsinf.Hang(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Inverse_trigonometric_functions"/>
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// angle between B and C
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Arccos(Entity a) => Arccosf.Hang(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Inverse_trigonometric_functions"/>
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// angle between A and C
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Arctan(Entity a) => Arctanf.Hang(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Inverse_trigonometric_functions"/>
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// angle between B and A
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Arccotan(Entity a) => Arccotanf.Hang(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Inverse_trigonometric_functions"/>
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// angle between C and B
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Arcsec(Entity a) => Arccosf.Hang(1 / a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Inverse_trigonometric_functions"/>
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// angle between C and A
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Arccosec(Entity a) => Arcsinf.Hang(1 / a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Natural_logarithm"/>
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Ln(Entity a) => Logf.Hang(e, a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Factorial"/>
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Factorial(Entity a) => Factorialf.Hang(a);

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Gamma_function"/>
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Gamma(Entity a) => Factorialf.Hang(a + 1);

        /// <summary>
        /// Creates an instance of variable entity.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static VariableEntity Var(string name) => new VariableEntity(name);

        /// <summary>
        /// Creates a complex instance of Number (not NumberEntity!)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete("Use Number.Create or implicit construction instead")]
        public static Number Num(EDecimal a, EDecimal b) => ComplexNumber.Create(a, b);

        /// <summary>
        /// Creates a real instance of Number (not NumberEntity!)
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete("Use Number.Create or implicit construction instead")]
        public static ComplexNumber Num(EDecimal a) => RealNumber.Create(a);

        // List of public constants
        public static readonly VariableEntity e = "e";
        public static readonly ComplexNumber i = ComplexNumber.ImaginaryOne;
        public static readonly VariableEntity pi = "pi";

        /// <summary>
        /// Converts an expression from a string
        /// </summary>
        /// <param name="expr">
        /// String expression, for example, "2 * x + 3 + sqrt(x)"
        /// </param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity FromString(string expr)
            => Parser.Parse(expr);

        /// <summary>
        /// Translates num10 into another number system
        /// </summary>
        /// <param name="num"></param>
        /// <param name="N"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToBaseN(RealNumber num, int N) 
            => NumberSystem.ToBaseN(num.Value, N);

        /// <summary>
        /// Translates num into 10 number system
        /// </summary>
        /// <param name="num"></param>
        /// <param name="N"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RealNumber FromBaseN(string num, int N)
            => NumberSystem.FromBaseN(num, N);

        /// <summary>
        /// Returns LaTeX code of the argument
        /// </summary>
        /// <param name="latexiseable"></param>
        /// <returns></returns>
        public static string Latex(ILatexiseable latexiseable)
            => latexiseable.Latexise();

        /// <summary>
        /// All operations for Number and its derived classes are available from here
        /// </summary>
        public static class Numbers
        {
            /*
             *
             * This list represents the only possible way to explicitly create numeric instances
             * It will automatically downcast the result for you, so that 1.0 is an IntegerNumber
             * To avoid it, you may temporarily disable it
             *
             *   MathS.Settings.DowncastingEnabled.Set(false);
             *     var yourNum = Number.Create(1.0);
             *   MathS.Settings.DowncastingEnabled.Unset();
             *
             */
            /// <summary>
            /// Creates an instance of ComplexNumber from System.Numerics.Complex
            /// </summary>
            /// <param name="value"></param>
            /// <returns>
            /// ComplexNumber
            /// </returns>
            public static ComplexNumber Create(Complex value) =>
                Create(EDecimal.FromDouble(value.Real), EDecimal.FromDouble(value.Imaginary));

            /// <summary>
            /// Creates an instance of IntegerNumber from long
            /// </summary>
            /// <param name="value"></param>
            /// <returns>
            /// IntegerNumber
            /// </returns>
            public static IntegerNumber Create(long value) => IntegerNumber.Create(value);

            /// <summary>
            /// Creates an instance of IntegerNumber from System.Numerics.EInteger
            /// </summary>
            /// <param name="value"></param>
            /// <returns>
            /// IntegerNumber
            /// </returns>
            public static IntegerNumber Create(EInteger value) => IntegerNumber.Create(value);

            /// <summary>
            /// Creates an instance of IntegerNumber from int
            /// </summary>
            /// <param name="value"></param>
            /// <returns>
            /// IntegerNumber
            /// </returns>
            public static IntegerNumber Create(int value) => IntegerNumber.Create(value);

            /// <summary>
            /// Creates an instance of RationalNumber of two IntegerNumbers
            /// </summary>
            /// <returns>
            /// RationalNumber
            /// </returns>
            public static RationalNumber CreateRational(EInteger numerator, EInteger denominator)
                => RationalNumber.Create(numerator, denominator);
            /// <summary>
            /// Creates an instance of RationalNumber of two IntegerNumbers
            /// </summary>
            /// <returns>
            /// RationalNumber
            /// </returns>
            public static RationalNumber Create(ERational rational) => RationalNumber.Create(rational);

            /// <summary>
            /// Creates an instance of RealNumber from EDecimal
            /// </summary>
            /// <param name="value"></param>
            /// <returns>
            /// RealNumber
            /// </returns>
            public static RealNumber Create(EDecimal value) => RealNumber.Create(value);

            /// <summary>
            /// Creates an instance of RealNumber from double
            /// </summary>
            /// <param name="value"></param>
            /// <returns>
            /// RealNumber
            /// </returns>
            public static RealNumber Create(double value) => RealNumber.Create(EDecimal.FromDouble(value));

            /// <summary>
            /// Creates an instance of ComplexNumber from two RealNumbers
            /// </summary>
            /// <param name="re">
            /// Real part of a desired complex number
            /// </param>
            /// <param name="im">
            /// Imaginary part of a desired complex number
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
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public static Tensor Matrix(int rows, int columns, params Entity[] values)
                => TensorFunctional.Matrix(rows, columns, values);

            /// <summary>
            /// Creates an instance of Matrix
            /// </summary>
            /// <param name="values">
            /// 2-D array of values
            /// </param>
            /// <returns></returns>
            public static Tensor Matrix(Entity[,] values)
                => TensorFunctional.Matrix(values);

            //public static Tensor Tensor()

            /// <summary>
            /// Creates an instance of vector
            /// </summary>
            /// <param name="p"></param>
            /// <returns></returns>
            public static Tensor Vector(params Entity[] values)
                => TensorFunctional.Vector(values);

            /// <summary>
            /// Returns dot product of two matrices
            /// </summary>
            /// <param name="A"></param>
            /// <param name="B"></param>
            /// <returns></returns>
            [Obsolete("Use MatrixMultiplication instead")]
            public static Tensor DotProduct(Tensor A, Tensor B) =>
                TensorFunctional.DotProduct(A, B);

            /// <summary>
            /// Returns multiplication of two matrices
            /// </summary>
            /// <param name="A"></param>
            /// <param name="B"></param>
            /// <returns></returns>
            public static Tensor MatrixMultiplication(Tensor A, Tensor B) =>
                new Tensor(GenTensor<Entity>.TensorMatrixMultiply(A.innerTensor, B.innerTensor));

            /// <summary>
            /// Returns scalar product of two matrices
            /// </summary>
            /// <param name="A"></param>
            /// <param name="B"></param>
            /// <returns></returns>
            public static Entity ScalarProduct(Tensor A, Tensor B) =>
                TensorFunctional.ScalarProduct(A, B);

        }

        /// <summary>
        /// A couple of settings allowing you to set some preferences for AM's algorithms
        /// To use those settings the syntax is
        /// MathS.Settings.SomeSetting.Set(5);   // Here you set a value to the setting
        /// ... // your code
        /// MathS.Settings.SomeSettings.Unset(); // Optional. Reverts the setting to the previous value
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
            /// MathS.Settings.NewtonSolver.Set(
            /// new NewtonSetting() {
            ///     From = (-10, -10),
            ///     To = (10, 10),
            ///     StepCount = (10, 10),
            ///     Precision = 30
            /// }
            /// );
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
            /// <param name="tree"></param>
            /// <returns></returns>
            public static Entity OptimizeTree(Entity tree)
                => TreeAnalyzer.Optimization.OptimizeTree(tree);

            /// <summary>
            /// Returns sympy interpretable format
            /// </summary>
            /// <param name="expr"></param>
            /// <returns></returns>
            public static string ToSympyCode(Entity expr)
                => ToSympy.GenerateCode(expr);

            /// <summary>
            /// Returns list of unique variables, for example 
            /// it extracts `x`, `goose` from (x + 2 * goose) - pi * x
            /// </summary>
            /// <param name="expr"></param>
            /// <returns></returns>
            public static Set GetUniqueVariables(Entity expr)
                => TreeAnalyzer.GetUniqueVariables(expr);

            /// <summary>
            /// Counts all nodes & subnodes that match criteria
            /// </summary>
            /// <param name="criteria"></param>
            /// <returns></returns>
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
            /// <returns></returns>
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
