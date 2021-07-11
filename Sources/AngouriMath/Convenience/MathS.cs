/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */
using System;
using System.Runtime.CompilerServices;
using PeterO.Numbers;
using AngouriMath.Functions.Algebra;
using AngouriMath.Functions.Boolean;
using System.Diagnostics.CodeAnalysis;
using AngouriMath.Convenience;
using AngouriMath.Core.Multithreading;
using System.Threading;
using AngouriMath.Core.Exceptions;

namespace AngouriMath
{
    using static Entity;
    using static Entity.Number;
    using NumericsComplex = System.Numerics.Complex;
    using GenTensor = GenericTensor.Core.GenTensor<Entity, Entity.Matrix.EntityTensorWrapperOperations>;
    using static Entity.Set;
    using static AngouriMath.MathS.UnsafeAndInternal;

    /// <summary>Use functions from this class</summary>
    public static partial class MathS
    {
        /// <summary>Use it in order to explore further number theory</summary>
        public static class NumberTheory
        {
            /// <summary>
            /// Returns entity standing for Euler phi function
            /// <a href="https://en.wikipedia.org/wiki/Euler%27s_totient_function"/>
            /// </summary>
            /// If integer x is non-positive, the result will be 0
            public static Entity Phi(Entity integer) => new Phif(integer);

            /// <summary>
            /// Count of all divisors of an integer
            /// </summary>
            public static Integer CountDivisors(Integer integer) => integer.CountDivisors();

            /// <summary>
            /// Factorization of integer
            /// </summary>
            public static IEnumerable<(Integer prime, Integer power)> Factorize(Integer integer) => integer.Factorize();

            /// <summary>
            /// Finds the greatest common divisor
            /// of two integers
            /// </summary>
            public static Integer GreatestCommonDivisor(Integer a, Integer b)
                => a.EInteger.Gcd(b.EInteger);

        }

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
        public static Matrix? SolveBooleanTable(Entity expression, params Variable[] variables)
            => BooleanSolver.SolveTable(expression, variables);


        /// <summary><a href="https://en.wikipedia.org/wiki/Trigonometric_functions"/></summary>
        /// <param name="a">Argument node of sine</param>
        /// <returns>Sine node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
        public static Entity Sin(Entity a) => new Sinf(a);

        /// <summary><a href="https://en.wikipedia.org/wiki/Trigonometric_functions"/></summary>
        /// <param name="a">Argument node of cosine</param>
        /// <returns>Cosine node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
        public static Entity Cos(Entity a) => new Cosf(a);

        /// <summary><a href="https://en.wikipedia.org/wiki/Trigonometric_functions"/></summary>
        /// <param name="a">Argument node of secant</param>
        /// <returns>Cosine node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
        public static Entity Sec(Entity a) => new Secantf(a);

        /// <summary><a href="https://en.wikipedia.org/wiki/Trigonometric_functions"/></summary>
        /// <param name="a">Argument node of cosecant</param>
        /// <returns>Cosine node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
        public static Entity Cosec(Entity a) => new Cosecantf(a);

        /// <summary><a href="https://en.wikipedia.org/wiki/Logarithm"/></summary>
        /// <param name="base">Base node of logarithm</param>
        /// <param name="x">Argument node of logarithm</param>
        /// <returns>Logarithm node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
        public static Entity Pow(Entity @base, Entity power) => new Powf(@base, power);

        /// <summary>Special case of <a href="https://en.wikipedia.org/wiki/Power_function"/></summary>
        /// <param name="a">The argument of which square root will be taken</param>
        /// <returns>Power node with (1/2) as the power</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
        public static Entity Sqrt(Entity a) => new Powf(a, Number.Rational.Create(1, 2));

        /// <summary>Special case of <a href="https://en.wikipedia.org/wiki/Power_function"/></summary>
        /// <param name="a">The argument of which cube root will be taken</param>
        /// <returns>Power node with (1/3) as the power</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
        public static Entity Cbrt(Entity a) => new Powf(a, Number.Rational.Create(1, 3));

        /// <summary>Special case of <a href="https://en.wikipedia.org/wiki/Power_function"/></summary>
        /// <param name="a">Argument to be squared</param>
        /// <returns>Power node with 2 as the power</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
        public static Entity Sqr(Entity a) => new Powf(a, 2);

        /// <summary><a href="https://en.wikipedia.org/wiki/Trigonometric_functions"/></summary>
        /// <param name="a">Argument node of which tangent will be taken</param>
        /// <returns>Tangent node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
        public static Entity Tan(Entity a) => new Tanf(a);

        /// <summary><a href="https://en.wikipedia.org/wiki/Trigonometric_functions"/></summary>
        /// <param name="a">Argument node of which cotangent will be taken</param>
        /// <returns>Cotangent node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
        public static Entity Cotan(Entity a) => new Cotanf(a);

        /// <summary><a href="https://en.wikipedia.org/wiki/Inverse_trigonometric_functions"/></summary>
        /// <param name="a">Argument node of which arcsine will be taken</param>
        /// <returns>Arcsine node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
        public static Entity Arcsin(Entity a) => new Arcsinf(a);

        /// <summary><a href="https://en.wikipedia.org/wiki/Inverse_trigonometric_functions"/></summary>
        /// <param name="a">Argument node of which arccosine will be taken</param>
        /// <returns>Arccosine node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
        public static Entity Arccos(Entity a) => new Arccosf(a);

        /// <summary><a href="https://en.wikipedia.org/wiki/Inverse_trigonometric_functions"/></summary>
        /// <param name="a">Argument node of which arctangent will be taken</param>
        /// <returns>Arctangent node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
        public static Entity Arctan(Entity a) => new Arctanf(a);

        /// <summary><a href="https://en.wikipedia.org/wiki/Inverse_trigonometric_functions"/></summary>
        /// <param name="a">Argument node of which arccotangent will be taken</param>
        /// <returns>Arccotangent node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
        public static Entity Arccotan(Entity a) => new Arccotanf(a);

        /// <summary><a href="https://en.wikipedia.org/wiki/Inverse_trigonometric_functions"/></summary>
        /// <param name="a">Argument node of which arcsecant will be taken</param>
        /// <returns>Arccosine node with the reciprocal of the argument</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
        public static Entity Arcsec(Entity a) => new Arcsecantf(a);

        /// <summary><a href="https://en.wikipedia.org/wiki/Inverse_trigonometric_functions"/></summary>
        /// <param name="a">Argument node of which arccosecant will be taken</param>
        /// <returns>Arcsine node with the reciprocal of the argument</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
        public static Entity Arccosec(Entity a) => new Arccosecantf(a);

        /// <summary>
        /// Is a special case of logarithm where the base equals
        /// <a href="https://en.wikipedia.org/wiki/E_(mathematical_constant)">e</a>:
        /// <a href="https://en.wikipedia.org/wiki/Natural_logarithm"/>
        /// </summary>
        /// <param name="a">Argument node of which natural logarithm will be taken</param>
        /// <returns>Logarithm node with base equal to e</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
        public static Entity Ln(Entity a) => new Logf(e, a);

        /// <summary><a href="https://en.wikipedia.org/wiki/Factorial"/></summary>
        /// <param name="a">Argument node of which factorial will be taken</param>
        /// <returns>Factorial node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
        public static Entity Factorial(Entity a) => new Factorialf(a);

        /// <summary><a href="https://en.wikipedia.org/wiki/Gamma_function"/></summary>
        /// <param name="a">Argument node of which gamma function will be taken</param>
        /// <returns>Factorial node with one added to the argument</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
        public static Entity Gamma(Entity a) => new Factorialf(a + 1);

        /// <summary>https://en.wikipedia.org/wiki/Sign_function</summary>
        /// <param name="a">Argument node of which Signum function will be taken</param>
        /// <returns>Signum node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
        public static Entity Signum(Entity a) => new Signumf(a);

        /// <summary>https://en.wikipedia.org/wiki/Absolute_value</summary>
        /// <param name="a">Argument node of which Abs function will be taken</param>
        /// <returns>Abs node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
        public static Entity Abs(Entity a) => new Absf(a);

        /// <summary>https://en.wikipedia.org/wiki/Negation</summary>
        /// <param name="a">Argument node of which Negation function will be taken</param>
        /// <returns>The Not node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
        public static Entity Negation(Entity a) => !a;

        /// <summary>
        /// This will be turned into <paramref name="expression"/> if the <paramref name="condition"/> is true,
        /// into NaN if <paramref name="condition"/> is false, and remain the same otherwise
        /// </summary>
        /// <param name="expression">The expression is extracted if the predicate is true</param>
        /// <param name="condition">Condition when the expression is defined</param>
        /// <returns>The Provided node</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
        public static Entity Provided(Entity expression, Entity condition)
            => expression.Provided(condition);

        /// <summary>
        /// This is a piecewisely defined function, which turns into a particular definition
        /// once there exists a case number N such that case[N].Predicate is turned into true and
        /// for all i less than N : case[i].Predicate is turned into false.
        /// 
        /// For example, Piecewise(new Providedf(a, b), new Providedf(d, false), new Providedf(f, true))
        /// will remain unchanged, because the first case is uncertain.
        /// 
        /// Piecewise(new Providedf(a, false), new Providedf(d, false), new Providedf(f, true))
        /// will turn into f
        /// 
        /// Piecewise(new Providedf(a, false), new Providedf(d, false), new Providedf(f, false))
        /// will turn into NaN
        /// </summary>
        /// <param name="cases">
        /// Cases, each of type Provided.
        /// </param>
        /// <param name="otherwise">
        /// An otherwise case. Will be intepreted as otherwise.Provided(true). Optional.
        /// </param>
        public static Entity Piecewise(IEnumerable<Providedf> cases, Entity? otherwise = null)
            => new Piecewise(otherwise is null ? cases : cases.Append(new Providedf(otherwise, true)));

        /// <summary>
        /// This is a piecewisely defined function, which turns into a particular definition
        /// once there exists a case number N such that case[N].Predicate is turned into true and
        /// for all i less than N : case[i].Predicate is turned into false.
        /// 
        /// For example, Piecewise((a, b), (d, false), (f, true))
        /// will remain unchanged, because the first case is uncertain.
        /// 
        /// Piecewise((a, false), (d, false), (f, true))
        /// will turn into f
        /// 
        /// Piecewise((a, false), (d, false), (f, false))
        /// will turn into NaN
        /// </summary>
        /// <param name="cases">
        /// Tuples of two expressions: an expression and a predicate
        /// </param>
        public static Entity Piecewise(params (Entity expression, Entity predicate)[] cases)
            => new Piecewise(cases.Select(c => new Providedf(c.expression, c.predicate)));

        /// <summary>
        /// Represents a few hyperbolic functions
        /// </summary>
        public static class Hyperbolic
        {
            /// <summary>https://en.wikipedia.org/wiki/Hyperbolic_functions</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
            public static Entity Sinh(Entity x) => (e.Pow(x) - e.Pow(-x)) / 2;

            /// <summary>https://en.wikipedia.org/wiki/Hyperbolic_functions</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
            public static Entity Cosh(Entity x) => (e.Pow(x) + e.Pow(-x)) / 2;

            /// <summary>https://en.wikipedia.org/wiki/Hyperbolic_functions</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
            public static Entity Tanh(Entity x) => (e.Pow(2 * x) - 1) / (e.Pow(2 * x) + 1);

            /// <summary>https://en.wikipedia.org/wiki/Hyperbolic_functions</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
            public static Entity Cotanh(Entity x) => (e.Pow(2 * x) + 1) / (e.Pow(2 * x) - 1);

            /// <summary>https://en.wikipedia.org/wiki/Hyperbolic_functions</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
            public static Entity Sech(Entity x) => 1 / Cosh(x);

            /// <summary>https://en.wikipedia.org/wiki/Hyperbolic_functions</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
            public static Entity Cosech(Entity x) => 1 / Sinh(x);

            /// <summary>https://en.wikipedia.org/wiki/Inverse_hyperbolic_functions</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
            public static Entity Arsinh(Entity x) => Ln(x + Sqrt(x.Pow(2) + 1));

            /// <summary>https://en.wikipedia.org/wiki/Inverse_hyperbolic_functions</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
            public static Entity Arcosh(Entity x) => Ln(x + Sqrt(x.Pow(2) - 1));

            /// <summary>https://en.wikipedia.org/wiki/Inverse_hyperbolic_functions</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
            public static Entity Artanh(Entity x) => 0.5 * Ln((1 + x) / (1 - x));

            /// <summary>https://en.wikipedia.org/wiki/Inverse_hyperbolic_functions</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
            public static Entity Arcotanh(Entity x) => 0.5 * Ln((1 - x) / (1 + x));

            /// <summary>https://en.wikipedia.org/wiki/Inverse_hyperbolic_functions</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
            public static Entity Arsech(Entity x) => Ln(1 / x + Sqrt(1 / Sqr(x) - 1));

            /// <summary>https://en.wikipedia.org/wiki/Inverse_hyperbolic_functions</summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining), NativeExport]
            public static Entity Arcosech(Entity x) => Ln(1 / x + Sqrt(1 / Sqr(x) + 1));
        }

        /// <summary>
        /// That is a collection of some series, expressed in a symbolic form
        /// </summary>
        public static class Series
        {
            /// <summary>
            /// Finds the symbolic expression of terms of the Maclaurin expansion of the given function,
            /// https://en.wikipedia.org/wiki/Taylor_series
            /// </summary>
            /// <param name="expr">
            /// The function to find the Taylor expansion of
            /// </param>
            /// <param name="degree">
            /// The degree of the resulting taylor polynomial (and the variable in the resulting series)
            /// </param>
            /// <param name="exprVariables">
            /// The variable/s to take the series over (and the variable the series will be over)
            /// (e. g. if you have expr = Sin("t"), then you may want to use "t" for this argument)
            /// </param>
            /// <returns>
            /// An expression in the polynomial form over the expression variables given in <paramref name="exprVariables"/>
            /// </returns>
            public static Entity Maclaurin(Entity expr, int degree, params Variable[] exprVariables)
                => Functions.Series.TaylorExpansion(expr, degree, exprVariables.Select(v => (v, v, (Entity)0)).ToArray());

            /// <summary>
            /// Finds the symbolic expression of terms of the Taylor expansion of the given function,
            /// https://en.wikipedia.org/wiki/Taylor_series
            /// </summary>
            /// <param name="expr">
            /// The function to find the Taylor expansion of
            /// </param>
            /// <param name="degree">
            /// The degree of the resulting taylor polynomial
            /// </param>
            /// <param name="exprVariables">
            /// The variable/s to take the series over (and the variable in the resulting series),
            /// plus the variable values at which the Taylor polynomial will be found
            /// (e. g. if you want to find the taylor polynomial of Sin("t") around t=1, then you may want to use ("t","1") for this argument)
            /// </param>
            /// <returns>
            /// An expression in the polynomial form over the expression variable/s given in <paramref name="exprVariables"/>
            /// </returns>
            public static Entity Taylor(Entity expr, int degree, params (Variable exprVariable, Entity point)[] exprVariables)
                => Functions.Series.TaylorExpansion(expr, degree, exprVariables.Select(v => (v.exprVariable, v.exprVariable, v.point)).ToArray());

            /// <summary>
            /// Finds the symbolic expression of terms of the Taylor expansion of the given function,
            /// https://en.wikipedia.org/wiki/Taylor_series
            /// </summary>
            /// <param name="expr">
            /// The function to find the Taylor expansion of
            /// </param>
            /// <param name="degree">
            /// The degree of the resulting taylor polynomial
            /// </param>
            /// <param name="exprToPolyVars">
            /// The variable/s to take the series over, the variable the series will be over,
            /// and the variable values at which the Taylor polynomial will be found
            /// (e. g. if you want to find the taylor polynomial of Sin("t") around t=1, and want
            ///  n to take that place in the series, then you may want to use ("t","n","1") for this argument)
            /// </param>
            /// <returns>
            /// An expression in the polynomial form over the poly variable/s given in <paramref name="exprToPolyVars"/>
            /// </returns>
            public static Entity Taylor(Entity expr, int degree, params (Variable exprVariable, Variable polyVariable, Entity point)[] exprToPolyVars)
                => Functions.Series.TaylorExpansion(expr, degree, exprToPolyVars);

            /// <summary>
            /// Finds the symbolic expression of terms of the Taylor expansion of the given function,
            /// https://en.wikipedia.org/wiki/Taylor_series
            /// 
            /// Do NOT call ToArray() or anything like this on the result of this method. It is 
            /// infinite iterator. To get a finite result as in a sum of finite number of terms,
            /// call TaylorExpansion
            /// </summary>
            /// <param name="expr">
            /// The function to find the Taylor expansion of
            /// </param>
            /// <param name="exprToPolyVars">
            /// The variable/s to take the series over, the variable the series will be over,
            /// and the variable values at which the Taylor polynomial will be found
            /// (e. g. if you want to find the taylor polynomial of Sin("t") around t=1, and want
            ///  n to take that place in the series, then you may want to use ("t","n","1") for this argument)
            /// </param>
            /// <returns>
            /// An infinite iterator over the terms of Taylor series of the given expression.
            /// </returns>
            public static IEnumerable<Entity> TaylorTerms(Entity expr, params (Variable exprVariable, Variable polyVariable, Entity point)[] exprToPolyVars)
            {
                if (exprToPolyVars.Length == 1)
                    return Functions.Series.SingleTaylorExpansionTerms(expr, exprToPolyVars[0].exprVariable, exprToPolyVars[0].polyVariable, exprToPolyVars[0].point);
                else
                    return Functions.Series.MultivariableTaylorExpansionTerms(expr, exprToPolyVars);
            }


        }


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

        /// <summary>https://en.wikipedia.org/wiki/Exclusive_or</summary>
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
        [ConstantField] public static readonly Variable e = Variable.e;
        // ReSharper disable once InconsistentNaming

        /// <summary>
        /// The imaginary one
        /// <a href="https://en.wikipedia.org/wiki/Imaginary_unit"/>
        /// </summary>
        [ConstantField] public static readonly Complex i = Complex.ImaginaryOne;

        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// The pi constant
        /// <a href="https://en.wikipedia.org/wiki/Pi"/>
        /// </summary>
        [ConstantField] public static readonly Variable pi = Variable.pi;

        // Undefined
        /// <summary>
        /// That is both undefined and indeterminite
        /// Any operation on NaN returns NaN
        /// </summary>
        [ConstantField] public static readonly Entity NaN = Real.NaN;

        /// <summary>
        /// The square identity matrix of size 1
        /// </summary>
        [ConstantField] public static readonly Matrix I_1 = IdentityMatrix(1);

        /// <summary>
        /// The square identity matrix of size 2
        /// </summary>
        [ConstantField] public static readonly Matrix I_2 = IdentityMatrix(2);

        /// <summary>
        /// The square identity matrix of size 3
        /// </summary>
        [ConstantField] public static readonly Matrix I_3 = IdentityMatrix(3);

        /// <summary>
        /// The square identity matrix of size 4
        /// </summary>
        [ConstantField] public static readonly Matrix I_4 = IdentityMatrix(4);

        /// <summary>
        /// The square zero matrix of size 1
        /// </summary>
        [ConstantField] public static readonly Matrix O_1 = ZeroMatrix(1);

        /// <summary>
        /// The square zero matrix of size 2
        /// </summary>
        [ConstantField] public static readonly Matrix O_2 = ZeroMatrix(2);

        /// <summary>
        /// The square zero matrix of size 3
        /// </summary>
        [ConstantField] public static readonly Matrix O_3 = ZeroMatrix(3);

        /// <summary>
        /// The square zero matrix of size 4
        /// </summary>
        [ConstantField] public static readonly Matrix O_4 = ZeroMatrix(4);


        /// <summary>Converts a <see cref="string"/> to an expression</summary>
        /// <param name="expr"><see cref="string"/> expression, for example, <code>"2 * x + 3 + sqrt(x)"</code></param>
        /// <param name="useCache">By default is true, it boosts performance if you have multiple uses of the same string,
        /// for example, 
        /// Entity expr = (Entity)"+oo" + "x".Limit("x", "+oo") * "+oo";
        /// First occurance will be parsed, others will be replaced with the cached entity
        /// </param>
        /// <returns>The parsed expression</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity FromString(string expr, bool useCache)
            => expr
                .LetLazy(out var parsed, Parser.Parse)
                .ReplaceWith(useCache)
                    switch
                    {
                        false => parsed.Value,
                        true =>
                            expr switch
                            {
                                "0" => Integer.Create(0),
                                "1" => Integer.Create(1),
                                "-1" => Integer.Create(-1),
                                "+oo" => Real.PositiveInfinity,
                                "-oo" => Real.NegativeInfinity,
                                _ => Settings.ExplicitParsingOnly.Value switch
                                    {
                                        false => stringToEntityCache.GetValue(expr, _ => parsed.Value),
                                        true => stringToEntityCacheExplicitOnly.GetValue(expr, _ => parsed.Value)
                                    } 
                            }
                    };
        private static ConditionalWeakTable<string, Entity> stringToEntityCacheExplicitOnly = new();
        private static ConditionalWeakTable<string, Entity> stringToEntityCache = new();

        /// <summary>Converts a <see cref="string"/> to an expression</summary>
        /// <param name="expr"><see cref="string"/> expression, for example, <code>"2 * x + 3 + sqrt(x)"</code></param>
        /// <returns>The parsed expression</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity FromString(string expr) => FromString(expr, useCache: true);
        
        /// <summary>
        /// Parses an expression silently, that is,
        /// without throwing an exception. Instead,
        /// it returns a Failure in case of encountered
        /// errors during parsing.
        /// </summary>
        /// <returns>
        /// Returns a type union of the successful result and
        /// failure, which is a type union of multiple reasons
        /// it may have failed.
        /// </returns>
        public static ParsingResult Parse(string source)
            => Parser.ParseSilent(source);

        /// <summary>Translates a <see cref="Number"/> in base 10 into base <paramref name="N"/></summary>
        /// <param name="num">A <see cref="Real"/> in base 10 to be translated into base <paramref name="N"/></param>
        /// <param name="N">The base to translate the number into</param>
        /// <returns>A <see cref="string"/> with the number in the required base</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToBaseN(Real num, int N) => BaseConversion.ToBaseN(num.EDecimal, N);

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
        /// using var _ = MathS.Settings.DowncastingEnabled.Set(false);
        /// var yourNum = Number.Create(1.0);
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

        /// <summary>
        /// Finds the determinant of the given matrix. If
        /// the matrix is non-square, returns null
        /// </summary>
        public static Entity? Det(Matrix m)
            => m.Determinant;

        /// <summary>Creates an instance of <see cref="Entity.Matrix"/>.</summary>
        /// <param name="values">
        /// A two-dimensional array of values.
        /// The first dimension is the row count, the second one is for columns.
        /// </param>
        /// <returns>A two-dimensional <see cref="Entity.Matrix"/> which is a matrix</returns>
        public static Matrix Matrix(Entity[,] values) => new(GenTensor.CreateMatrix(values));

        /// <summary>
        /// Creates an instance of matrix, where each cell's
        /// index is mapped to a value with the help of the
        /// mapping function.
        /// </summary>
        /// <param name="rowCount">
        /// The number of rows (corresponds to the first index).
        /// </param>
        /// <param name="colCount">
        /// The number of columns (corresponds to the second index).
        /// </param>
        /// <param name="map">
        /// The first argument of the mapping function
        /// function is the index of row, the second one for the 
        /// column index.
        /// 
        /// Indexing starts from 0.
        /// </param>
        /// <returns>
        /// A newly created matrix of the given size.
        /// </returns>
        public static Matrix Matrix(int rowCount, int colCount, Func<int, int, Entity> map)
            => new(GenTensor.CreateMatrix(rowCount, colCount, map));


        /// <summary>Creates an instance of <see cref="Entity.Matrix"/> that has one column.</summary>
        /// <param name="values">The cells of the <see cref="Entity.Matrix"/></param>
        /// <returns>A one-dimensional <see cref="Entity.Matrix"/> which is a vector</returns>
        public static Matrix Vector(params Entity[] values)
            => new Matrix(GenTensor.CreateTensor(new(values.Length, 1), arr => values[arr[0]]));

        /// <summary>
        /// Creates a zero square matrix
        /// </summary>
        public static Matrix ZeroMatrix(int size)
            => ZeroMatrix(size, size);

        /// <summary>
        /// Creates a zero square matrix
        /// </summary>
        public static Matrix ZeroMatrix(int rowCount, int columnCount)
            => new Matrix(GenTensor.CreateTensor(new(rowCount, columnCount), arr => 0));

        /// <summary>
        /// Creates a zero vector
        /// </summary>
        public static Matrix ZeroVector(int size)
            => new Matrix(GenTensor.CreateTensor(new(size, 1), arr => 0));

        /// <summary>
        /// Creates a 1x1 matrix of a given value. It will be simplified
        /// once InnerSimplified or Evaled are addressed
        /// </summary>
        /// <returns>
        /// A 1x1 matrix, which is also a 1-long vector, or just a scalar.
        /// </returns>
        public static Matrix Scalar(Entity value)
            => Vector(value);

        /// <summary>
        /// Creates a matrix from given rows
        /// </summary>
        /// <param name="vectors">
        /// There should be at least one row.
        /// All rows must have the same number
        /// of columns
        /// </param>
        public static Matrix MatrixFromRows(IEnumerable<Matrix> vectors)
        {
            if (!vectors.Any())
                throw new InvalidMatrixOperationException("No rows were provided");
            var tb = new MatrixBuilder(vectors.First().ColumnCount);
            foreach (var v in vectors)
                tb.Add(v);
            return tb.ToMatrix() ?? throw new AngouriBugException("Nullability should have been checked before");
        }

        /// <summary>
        /// Creates a matrix from given elements
        /// </summary>
        /// <param name="elements">
        /// There should be at least one row.
        /// All rows must have the same number
        /// of columns
        /// </param>
        public static Matrix MatrixFromIEnum2x2(IEnumerable<IEnumerable<Entity>> elements)
        {
            if (!elements.Any())
                throw new InvalidMatrixOperationException("No rows were provided");
            var tb = new MatrixBuilder(elements.First().Count());
            foreach (var v in elements)
                tb.Add(v);
            return tb.ToMatrix() ?? throw new AngouriBugException("Nullability should have been checked before");
        }

        /// <summary>
        /// Creates a closed interval (segment)
        /// </summary>
        public static Interval Interval(Entity left, Entity right) => new Interval(left, true, right, true);

        /// <summary>
        /// Creates an interval with custom endings
        /// </summary>
        public static Interval Interval(Entity left, bool leftClosed, Entity right, bool rightClosed) => new Interval(left, leftClosed, right, rightClosed);

        /// <summary>
        /// Creates a square identity matrix
        /// </summary>
        public static Matrix IdentityMatrix(int size)
            => Entity.Matrix.I(size);

        /// <summary>
        /// Creates a rectangular identity matrix
        /// with the given size
        /// </summary>
        public static Matrix IdentityMatrix(int rowCount, int columnCount)
            => Entity.Matrix.I(rowCount, columnCount);

        /// <summary>Classes and functions related to matrices are defined here</summary>
        public static class Matrices
        {
            /// <summary>
            /// Performs a pointwise multiplication operation,
            /// or throws exception if shapes mismatch
            /// </summary>
            public static Matrix PointwiseMultiplication(Matrix m1, Matrix m2)
                => (Matrix)new Matrix(GenTensor.PiecewiseMultiply(m1.InnerMatrix, m2.InnerMatrix)).InnerSimplified;

            /// <summary>
            /// Creates an instance of <see cref="Entity.Matrix"/> that is a matrix.
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
            /// <returns>A two-dimensional <see cref="Entity.Matrix"/> which is a matrix</returns>
            public static Matrix Matrix(int rows, int columns, params Entity[] values) =>
                new(GenTensor.CreateMatrix(rows, columns, (x, y) => values[x * columns + y]));

            /// <summary>Creates an instance of <see cref="Entity.Matrix"/> that is a matrix.</summary>
            /// <param name="values">A two-dimensional array of values</param>
            /// <returns>A two-dimensional <see cref="Entity.Matrix"/> which is a matrix</returns>
            [Obsolete("Use MathS.Matrix instead")]
            public static Matrix Matrix(Entity[,] values) => new(GenTensor.CreateMatrix(values));

            /// <summary>Creates an instance of <see cref="Entity.Matrix"/> that is a vector.</summary>
            /// <param name="values">The cells of the <see cref="Entity.Matrix"/></param>
            /// <returns>A one-dimensional <see cref="Entity.Matrix"/> which is a vector</returns>
            [Obsolete("Use MathS.Vector instead")]
            public static Matrix Vector(params Entity[] values)
            {
                var arr = new Entity[values.Length, 1];
                for (int i = 0; i < values.Length; i++)
                    arr[i, 0] = values[i];
                return new(GenTensor.CreateMatrix(arr));
            }

            /// <summary>Returns the dot product of two <see cref="Entity.Matrix"/>s that are matrices.</summary>
            /// <param name="A">First matrix (its width is the result's width)</param>
            /// <param name="B">Second matrix (its height is the result's height)</param>
            /// <returns>A two-dimensional <see cref="Entity.Matrix"/> (matrix) as a result of symbolic multiplication</returns>
            [Obsolete("Use operator * instead")]
            public static Matrix DotProduct(Matrix A, Matrix B) => new(GenTensor.MatrixMultiply(A.InnerMatrix, B.InnerMatrix));

            /// <summary>Returns the dot product of two <see cref="Entity.Matrix"/>s that are matrices.</summary>
            /// <param name="A">First matrix (its width is the result's width)</param>
            /// <param name="B">Second matrix (its height is the result's height)</param>
            /// <returns>A two-dimensional <see cref="Entity.Matrix"/> (matrix) as a result of symbolic multiplication</returns>
            [Obsolete("Use operator * instead")]
            public static Matrix MatrixMultiplication(Matrix A, Matrix B) => new(GenTensor.TensorMatrixMultiply(A.InnerMatrix, B.InnerMatrix));

            /// <summary>Returns the scalar product of two <see cref="Entity.Matrix"/>s that are vectors with the same length.</summary>
            /// <param name="a">First vector (order does not matter)</param>
            /// <param name="b">Second vector</param>
            /// <returns>The resulting scalar which is an <see cref="Entity"/> and not a <see cref="Entity.Matrix"/></returns>
            [Obsolete("Use a.T * b for the same purpose")]
            public static Entity ScalarProduct(Matrix a, Matrix b) => GenTensor.VectorDotProduct(a.InnerMatrix, b.InnerMatrix);

            /// <summary>
            /// Creates a closed interval (segment)
            /// </summary>
            [Obsolete("Use MathS.Interval instead")]
            public static Interval Interval(Entity left, Entity right) => new Interval(left, true, right, true);

            /// <summary>
            /// Creates an interval with custom endings
            /// </summary>
            [Obsolete("Use MathS.Interval instead")]
            public static Interval Interval(Entity left, bool leftClosed, Entity right, bool rightClosed) => new Interval(left, leftClosed, right, rightClosed);
        }

        /// <summary>
        /// A couple of settings allowing you to set some preferences for AM's algorithms
        /// To use these settings the syntax is
        /// <code>
        /// using var _ = MathS.Settings.SomeSetting.Set(5 /* Here you set a value to the setting */);
        /// // here you write your code normally
        /// </code>
        /// </summary>
        public static partial class Settings
        {

            /// <summary>
            /// Determine if it should only parse if it is explicit
            /// </summary>
            public static Setting<bool> ExplicitParsingOnly => explicitParsingOnly ??= false;
            [ThreadStatic] private static Setting<bool>? explicitParsingOnly;
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
            public static Setting<Func<Entity, double>> ComplexityCriteria =>
                complexityCriteria ??= new Func<Entity, double>(expr =>
                {
                    // Those are of the 2nd power to avoid problems with floating numbers
                    static double TinyWeight(double w)  => w * 0.5;
                    static double MinorWeight(double w) => w * 1.0;
                    static double Weight(double w)      => w * 2.0;
                    static double MajorWeight(double w) => w * 4.0;
                    static double HeavyWeight(double w) => w * 8.0;
                    static double ExtraHeavyWeight(double w) => w * 12.0;

                    // Number of nodes
                    var res = Weight(expr.Complexity);

                    // Number of variables
                    res += Weight(expr.Nodes.Count(entity => entity is Variable));

                    // Number of divides
                    res += MinorWeight(expr.Nodes.Count(entity => entity is Divf));

                    // Number of rationals with unit numerator
                    res += Weight(expr.Nodes.Count(entity => entity is Rational rat and not Integer 
                        && (rat.Numerator == 1 || rat.Numerator == -1)));

                    // Number of negative powers
                    res += HeavyWeight(expr.Nodes.Count(entity => entity is Powf(_, Real { IsNegative: true })));

                    // Number of logarithms
                    res += TinyWeight(expr.Nodes.Count(entity => entity is Logf));

                    // Number of phi functions
                    res += ExtraHeavyWeight(expr.Nodes.Count(entity => entity is Phif));

                    // Number of negative reals
                    res += MajorWeight(expr.Nodes.Count(entity => entity is Real { IsNegative: true }));

                    // 0 < x is bad. x > 0 is good.
                    res += Weight(expr.Nodes.Count(entity => entity is ComparisonSign && entity.DirectChildren[0] == 0));

                    return res;
                });
            [ThreadStatic] private static Setting<Func<Entity, double>>? complexityCriteria;

            /// <summary>
            /// Settings for the Newton-Raphson's root-search method
            /// e.g.
            /// <code>
            /// using var _ = MathS.Settings.NewtonSolver.Set(new NewtonSetting {
            ///     From = (-10, -10),
            ///     To = (10, 10),
            ///     StepCount = (10, 10),
            ///     Precision = 30
            /// });
            /// ...
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
            public static Setting<long> MaxExpansionTermCount => maxExpansionTermCount ??= 2_000;
            [ThreadStatic] private static Setting<long>? maxExpansionTermCount;

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
            [NotNullWhen(true)]
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
        /// It supports intersection, union, subtraction
        ///             
        /// <see cref="Set"/>
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


            /// <summary>
            /// Computes Euler phi function
            /// <a href="https://en.wikipedia.org/wiki/Euler%27s_totient_function"/>
            /// </summary>
            /// If integer x is non-positive, the result will be 0
            public static Integer Phi(Integer integer) => integer.Phi();
            
            /// <summary>
            /// Finds the symbolic form of sine, if can
            /// For example, sin(9/14) is sin(1/2 + 1/7) which
            /// can be expanded as a sine of sum and hence
            /// an analytical (symbolic) form.
            /// </summary>
            /// <param name="angle">
            /// The angle in radians
            /// </param>
            /// <returns>
            /// The sine's symbolic form
            /// or null if cannot find it
            /// </returns>
            public static Entity? SymbolicFormOfSine(Entity angle)
                => TrigonometricAngleExpansion.SymbolicFormOfSine(angle)?.InnerSimplified;
            
            /// <summary>
            /// Finds the symbolic form of cosine, if can
            /// For example, cos(9/14) is cos(1/2 + 1/7) which
            /// can be expanded as a cosine of sum and hence
            /// an analytical (symbolic) form.
            /// </summary>
            /// <param name="angle">
            /// The angle in radians
            /// </param>
            /// <returns>
            /// The cosine's symbolic form
            /// or null if cannot find it
            /// </returns>
            public static Entity? SymbolicFormOfCosine(Entity angle)
                => TrigonometricAngleExpansion.SymbolicFormOfCosine(angle)?.InnerSimplified;
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
        [SuppressMessage("Style", "IDE1006:Naming Styles",
            Justification = "Lowercase constants as written in Mathematics")]
        public static class DecimalConst
        {
            /// <summary><a href="https://en.wikipedia.org/wiki/Pi"/></summary>
            public static EDecimal pi =>
                InternalAMExtensions.ConstantCache.Lookup(Settings.DecimalPrecisionContext).Pi;

            /// <summary><a href="https://en.wikipedia.org/wiki/E_(mathematical_constant)"/></summary>
            public static EDecimal e =>
                InternalAMExtensions.ConstantCache.Lookup(Settings.DecimalPrecisionContext).E;
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
            public static Matrix? BuildTruthTable(Entity expression, params Variable[] variables)
                => BooleanSolver.BuildTruthTable(expression, variables);


            /// <summary>
            /// Creates a boolean
            /// </summary>
            public static Entity.Boolean Create(bool b)
                => Entity.Boolean.Create(b);
        }

        /// <summary>
        /// A few functions convenient to use in industrial projects
        /// to keep the system more reliable and distribute computations
        /// to other threads
        /// </summary>
        public static class Multithreading
        {
            /// <summary>
            /// Sets the thread-local cancellation token
            /// </summary>
            /// <param name="token"></param>
            public static void SetLocalCancellationToken(CancellationToken token) => MultithreadingFunctional.SetLocalCancellationToken(token);
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
                    var children = TreeAnalyzer.SmartExpandOver(linChild, n => n.ContainsNode(x));
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
    }
}
