
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
using Number = AngouriMath.Core.Numerix.Number;

namespace AngouriMath
{
    /// <summary>
    /// Use functions from this class
    /// </summary>
    public static partial class MathS
    {
        public static Entity Quack(Entity expr, VariableEntity x)
            => CommonDenominatorSolver.FindCD(expr, x);

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
        /// Will be soon!
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
        /// https://en.wikipedia.org/wiki/Trigonometric_functions
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// A / C
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Sin(Entity a) => Sinf.Hang(a);

        /// <summary>
        /// https://en.wikipedia.org/wiki/Trigonometric_functions
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// B / C
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Cos(Entity a) => Cosf.Hang(a);

        /// <summary>
        /// https://en.wikipedia.org/wiki/Logarithm
        /// </summary>
        /// <param name="@base"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Log(Entity @base, Entity x) => Logf.Hang(@base, x);

        /// <summary>
        /// https://en.wikipedia.org/wiki/Power_function
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
        public static Entity Sqrt(Entity a) => Powf.Hang(a, Number.CreateRational(1, 2));

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
        /// https://en.wikipedia.org/wiki/Trigonometric_functions
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// A / B
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Tan(Entity a) => Tanf.Hang(a);

        /// <summary>
        /// https://en.wikipedia.org/wiki/Trigonometric_functions
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// B / A
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Cotan(Entity a) => Cotanf.Hang(a);

        /// <summary>
        /// https://en.wikipedia.org/wiki/Trigonometric_functions
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// C / B
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Sec(Entity a) => 1 / Cos(a);

        /// <summary>
        /// https://en.wikipedia.org/wiki/Trigonometric_functions
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// C / A
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Cosec(Entity a) => 1 / Sin(a);

        /// <summary>
        /// This function is every interesting for ASC (https://asc-community.org)
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// x * sin(x)
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity B(Entity a) => a * Sin(a);

        /// <summary>
        /// This function is every interesting for ASC (https://asc-community.org)
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// x * cos(x)
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity TB(Entity a) => a * Cos(a);

        /// <summary>
        /// https://en.wikipedia.org/wiki/Inverse_trigonometric_functions
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// angle between A and C
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Arcsin(Entity a) => Arcsinf.Hang(a);

        /// <summary>
        /// https://en.wikipedia.org/wiki/Inverse_trigonometric_functions
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// angle between B and C
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Arccos(Entity a) => Arccosf.Hang(a);

        /// <summary>
        /// https://en.wikipedia.org/wiki/Inverse_trigonometric_functions
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// angle between A and C
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Arctan(Entity a) => Arctanf.Hang(a);

        /// <summary>
        /// https://en.wikipedia.org/wiki/Inverse_trigonometric_functions
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// angle between B and A
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Arccotan(Entity a) => Arccotanf.Hang(a);

        /// <summary>
        /// https://en.wikipedia.org/wiki/Inverse_trigonometric_functions
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// angle between C and B
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Arcsec(Entity a) => Arccosf.Hang(1 / a);

        /// <summary>
        /// https://en.wikipedia.org/wiki/Inverse_trigonometric_functions
        /// </summary>
        /// <param name="a"></param>
        /// <returns>
        /// angle between C and A
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Arccosec(Entity a) => Arcsinf.Hang(1 / a);

        /// <summary>
        /// https://en.wikipedia.org/wiki/Natural_logarithm
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Ln(Entity a) => Logf.Hang(e, a);

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
        public static Number Num(decimal a, decimal b) => Number.Create(a, b);

        /// <summary>
        /// Creates a real instance of Number (not NumberEntity!)
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete("Use Number.Create or implicit construction instead")]
        public static ComplexNumber Num(decimal a) => Number.Create(a);

        /// <summary>
        /// List of public constants
        /// </summary>
        public static readonly VariableEntity e = "e";
        public static readonly ComplexNumber i = Number.Create(0, 1.0);
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
        public static string ToBaseN(decimal num, int N) 
            => NumberSystem.ToBaseN(num, N);

        /// <summary>
        /// Translates num into 10 number system
        /// </summary>
        /// <param name="num"></param>
        /// <param name="N"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static decimal FromBaseN(string num, int N)
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
            /// <summary>
            /// Creates an instance of ComplexNumber from System.Numerics.Complex
            /// </summary>
            /// <param name="value"></param>
            /// <returns>
            /// ComplexNumber
            /// </returns>
            public static ComplexNumber Create(Complex value)
                => Number.Functional.Downcast(new ComplexNumber(value)) as ComplexNumber;

            /// <summary>
            /// Creates an instance of IntegerNumber from long
            /// </summary>
            /// <param name="value"></param>
            /// <returns>
            /// IntegerNumber
            /// </returns>
            public static IntegerNumber Create(long value)
                => Number.Functional.Downcast(new IntegerNumber((BigInteger)value)) as IntegerNumber;

            /// <summary>
            /// Creates an instance of IntegerNumber from System.Numerics.BigInteger
            /// </summary>
            /// <param name="value"></param>
            /// <returns>
            /// IntegerNumber
            /// </returns>
            public static IntegerNumber Create(BigInteger value)
                => Number.Functional.Downcast(new IntegerNumber(value)) as IntegerNumber;

            /// <summary>
            /// Creates an instance of IntegerNumber from int
            /// </summary>
            /// <param name="value"></param>
            /// <returns>
            /// IntegerNumber
            /// </returns>
            public static IntegerNumber Create(int value)
                => Number.Functional.Downcast(new IntegerNumber((BigInteger)value)) as IntegerNumber;

            /// <summary>
            /// Creates an instance of RationalNumber of two IntegerNumbers
            /// </summary>
            /// <param name="numerator"></param>
            /// <param name="denominator"></param>
            /// <returns>
            /// RationalNumber
            /// </returns>
            public static RationalNumber CreateRational(IntegerNumber numerator, IntegerNumber denominator)
                => Number.Functional.Downcast(new RationalNumber(numerator, denominator)) as RationalNumber;

            /// <summary>
            /// Creates an instance of RealNumber from decimal
            /// </summary>
            /// <param name="value"></param>
            /// <returns>
            /// RealNumber
            /// </returns>
            public static RealNumber Create(decimal value)
                => Number.Functional.Downcast(new RealNumber(value)) as RealNumber;

            /// <summary>
            /// Creates an instance of RealNumber from double
            /// </summary>
            /// <param name="value"></param>
            /// <returns>
            /// RealNumber
            /// </returns>
            public static RealNumber Create(double value)
                => Number.Functional.Downcast(new RealNumber(value)) as RealNumber;

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
            public static ComplexNumber Create(RealNumber re, RealNumber im)
                => Number.Functional.Downcast(new ComplexNumber(re, im)) as ComplexNumber;

            /// <summary>
            /// If you need an indefinite value of a real number, use this
            /// Number.Create(RealNumber.UndefinedState.POSITIVE_INFINITY)
            /// Number.Create(RealNumber.UndefinedState.NEGATIVE_INFINITY)
            /// Number.Create(RealNumber.UndefinedState.NAN)
            /// </summary>
            /// <param name="state"></param>
            /// <returns></returns>
            public static RealNumber Create(RealNumber.UndefinedState state)
                => new RealNumber(state);

            /// <summary>
            /// If you need an indefinite value of a complex number, e. g.
            /// Number.Create(RealNumber.UndefinedState.POSITIVE_INFINITY, RealNumber.UndefinedState.NEGATIVE_INFINITY)
            /// -> +oo + -ooi
            /// </summary>
            /// <returns></returns>
            public static ComplexNumber Create(RealNumber.UndefinedState realState, RealNumber.UndefinedState imaginaryState)
                => Number.Create(Number.Create(realState), Number.Create(imaginaryState));
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
            public static Tensor DotProduct(Tensor A, Tensor B) =>
                TensorFunctional.DotProduct(A, B);

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
        public static class Settings
        {
            /// <summary>
            /// Enables downcasting. Not recommended to turn off, disabling might be only useful for some calculations
            /// </summary>
            public static Setting<bool> DowncastingEnabled { get; } = true;

            /// <summary>
            /// Amount of iterations allowed for attempting to cast to a rational
            /// The more iterations, the larger fraction could be calculated
            /// </summary>
            public static Setting<int> FloatToRationalIterCount { get; } = 5;

            /// <summary>
            /// If a numerator or denominator is too large, it's suspended to better keep the real number instead of casting
            /// </summary>
            public static Setting<long> MaxAbsNumeratorOrDenominatorValue { get; } = 100000000;

            /// <summary>
            /// Sets threshold for comparison
            /// For example, if you don't need precision higher than 6 digits after .,
            /// you can set it to 1.0e-6 so 1.0000000 == 0.9999999
            /// </summary>
            public static Setting<decimal> PrecisionErrorCommon { get; } = 1.0e-6m;

            /// <summary>
            /// Numbers whose absolute value is less than PrecisionErrorZeroRange are considered zeros
            /// </summary>
            public static Setting<decimal> PrecisionErrorZeroRange { get; } = 1.0e-16m;

            /// <summary>
            /// If you only need analytical solutions and an empty set if no analytical solutions were found, disable Newton's method
            /// </summary>
            public static Setting<bool> AllowNewton { get; } = true;

            /// <summary>
            /// Criteria for simplifier so you could control which expressions are considered easier by you
            /// </summary>
            public static Setting<Func<Entity, int>> ComplexityCriteria { get; } = Const.DefaultComplexityCriteria;

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
            public static Setting<NewtonSetting> NewtonSolver { get; set; } = new NewtonSetting();
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
            /// true if success
            /// false otherwise (do not access dst in this case, it's undefined)
            /// </returns>
            public static bool TryPolynomial(Entity expr, VariableEntity variable, out Entity dst)
                => Functions.Utils.TryPolynomial(expr, variable, out dst);


            /// <summary>
            /// Checks tree for some unexpected bad occasions
            /// Throws SysException's children
            /// If you need a message, it's better to write
            /// try
            /// {
            ///     MathS.CheckTree(a);
            /// }
            /// catch (SysException e)
            /// {
            ///     Console.WriteLine(e.Message);
            /// }
            /// </summary>
            /// <param name="expr"></param>
            /// <returns></returns>
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

            /// <summary>
            /// Returns a set of all complex numbers
            /// </summary>
            /// <returns></returns>
            public static Set C()
                => Set.C();

            /// <summary>
            /// Returns a set of all real numbers
            /// </summary>
            /// <returns></returns>
            public static Set R()
                => Set.R();

            /// <summary>
            /// Creats a set that you can fill with elements
            /// Later on, you may add an Interval if wish
            /// </summary>
            /// <param name="entities"></param>
            /// <returns></returns>
            public static Set Finite(params Entity[] entities)
                => Set.Finite(entities);

            /// <summary>
            /// Creates an interval
            /// To modify it, use
            /// Interval(3, 4).SetLeftClosed (see more alike functions in set documentation)
            /// </summary>
            /// <param name="from"></param>
            /// <param name="to"></param>
            /// <returns></returns>
            public static IntervalPiece Interval(Entity from, Entity to)
                => Piece.Interval(from, to).AsInterval();

            /// <summary>
            /// Creates an element for set
            /// One can be created implicitly,
            /// Piece a = 3;
            /// </summary>
            /// <param name="element"></param>
            /// <returns></returns>
            public static OneElementPiece Element(Entity element)
                => new OneElementPiece(element);
        }

        public static class DecimalConst
        {
            /// <summary>
            /// Pi constant
            /// </summary>
            public static readonly decimal pi = 3.14159_26535_89793_23846_26433m;

            /// <summary>
            /// E constant
            /// </summary>
            public static readonly decimal e  = 2.71828_18284_59045_23536_02874m;
        }
    }
}
