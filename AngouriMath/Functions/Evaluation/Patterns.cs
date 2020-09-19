
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
using System.Linq;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;
using static AngouriMath.Entity.Boolean;

namespace AngouriMath.Functions
{
    internal static class Patterns
    {
        // I really want variable declarations under or patterns: https://github.com/dotnet/csharplang/issues/3740
        // Currently they fail with error CS8780: A variable may not be declared within a 'not' or 'or' pattern.
        /// <summary>a ^ (-1) => 1 / a</summary>
        internal static Entity InvertNegativePowers(Entity expr) =>
            expr is Powf(var @base, Integer { IsNegative: true } pow)
            ? 1 / MathS.Pow(@base, -1 * pow)
            : expr;
        /// <summary>1 + (-x) => 1 - x</summary>
        internal static Entity InvertNegativeMultipliers(Entity expr) =>
            expr is Sumf(var any1, Mulf(Real { IsNegative: true } const1, var any2))
            ? any1 - (-1 * const1) * any2
            : expr;
        /// <summary>(x + a)! / (x + b)! -> (x+b+1)*(x+b+2)*...*(x+a)</summary>
        internal static Entity ExpandFactorialDivisions(Entity expr)
        {
            Entity ExpandFactorialDivisions(Entity x, Entity x2, Number num, Number den)
            {
                static Entity Add(Entity a, Number b) =>
                    b is Integer(0) ? a : a + b;
                if (x == x2
                    && num - den is Integer { EInteger: var diff }
                    && !diff.IsZero && diff.Abs() < 20) // We don't want to expand (x+100)!/x!
                    if (diff > 0) // e.g. (x+3)!/x! = (x+1)(x+2)(x+3)
                    {
                        var expr = Add(x, den + 1);
                        for (var i = 2; i <= diff; i++)
                            expr *= Add(x, den + i);
                        return expr;
                    }
                    else // e.g. x!/(x+3)! = 1/(x+1)/(x+2)/(x+3)
                    {
                        diff = -diff;
                        var expr = 1 / Add(x, num + 1);
                        for (var i = 2; i <= diff; i++)
                            expr /= Add(x, num + i);
                        return expr;
                    }
                return expr;
            }
            return expr switch
            {
                Divf(Factorialf(Sumf(var any1, Number const1)), Factorialf(Sumf(var any1a, Number const2)))
                    => ExpandFactorialDivisions(any1, any1a, const1, const2),
                Divf(Factorialf(Sumf(var any1, Number const1)), Factorialf(Sumf(Number const2, var any1a)))
                    => ExpandFactorialDivisions(any1, any1a, const1, const2),
                Divf(Factorialf(Sumf(Number const1, var any1)), Factorialf(Sumf(var any1a, Number const2)))
                    => ExpandFactorialDivisions(any1, any1a, const1, const2),
                Divf(Factorialf(Sumf(Number const1, var any1)), Factorialf(Sumf(Number const2, var any1a)))
                    => ExpandFactorialDivisions(any1, any1a, const1, const2),
                Divf(Factorialf(var any1), Factorialf(Sumf(var any1a, Number const2)))
                    => ExpandFactorialDivisions(any1, any1a, 0, const2),
                Divf(Factorialf(var any1), Factorialf(Sumf(Number const2, var any1a)))
                    => ExpandFactorialDivisions(any1, any1a, 0, const2),
                Divf(Factorialf(Sumf(var any1, Number const1)), Factorialf(var any1a))
                    => ExpandFactorialDivisions(any1, any1a, const1, 0),
                Divf(Factorialf(Sumf(Number const1, var any1)), Factorialf(var any1a))
                    => ExpandFactorialDivisions(any1, any1a, const1, 0),
                _ => expr
            };
        }

        // https://en.wikipedia.org/wiki/Reflection_formula
        // (z-1)! (-z)! -> Γ(z) Γ(1 - z) = π/sin(π z), z ∉ ℤ // actually, when z ∈ ℤ, both sides include division by zero, so we can still replace
        // Replace z with -z => z! (-z-1)! = π/sin(-π z)
        // TODO: Modify the complexity criteria to rank non-elementary functions more complex than elementary functions
        //       so that this formula can be used to simplify
        // TODO: Other than the reflection formula,
        // (z-1)! (z-1/2)! -> Γ(z) Γ(z + 1/2) = 2^(1 - 2 z) sqrt(π) Γ(2 z) -> 2^(1 - 2 z) sqrt(π) (2 z - 1)!
        // is also another possible simplification
        /// <summary>(x-1)! x -> x!, x! (x+1) -> (x+1)!, etc. <!--as well as z! (-z-1)! -> -π/sin(π z)--></summary>
        internal static Entity FactorizeFactorialMultiplications(Entity expr)
        {
            Entity FactorizeFactorialMultiplications(Entity x, Entity x2, Number factConst, Number @const) =>
                x == x2 && factConst + 1 == @const ? new Factorialf(x + @const) : expr;
            return expr switch
            {
                Mulf(Factorialf(Sumf(var any1, Number const1)), Sumf(var any1a, Number const2)) =>
                    FactorizeFactorialMultiplications(any1, any1a, const1, const2),
                Mulf(Factorialf(Sumf(Number const1, var any1)), Sumf(var any1a, Number const2)) =>
                    FactorizeFactorialMultiplications(any1, any1a, const1, const2),
                Mulf(Factorialf(Sumf(var any1, Number const1)), Sumf(Number const2, var any1a)) =>
                    FactorizeFactorialMultiplications(any1, any1a, const1, const2),
                Mulf(Factorialf(Sumf(Number const1, var any1)), Sumf(Number const2, var any1a)) =>
                    FactorizeFactorialMultiplications(any1, any1a, const1, const2),
                Mulf(Factorialf(var any1), Sumf(var any1a, Number const2)) =>
                    FactorizeFactorialMultiplications(any1, any1a, 0, const2),
                Mulf(Factorialf(var any1), Sumf(Number const2, var any1a)) =>
                    FactorizeFactorialMultiplications(any1, any1a, 0, const2),
                Mulf(Factorialf(Sumf(var any1, Number const1)), var any1a) =>
                    FactorizeFactorialMultiplications(any1, any1a, const1, 0),
                Mulf(Factorialf(Sumf(Number const1, var any1)), var any1a) =>
                    FactorizeFactorialMultiplications(any1, any1a, const1, 0),
                _ => expr
            };
        }
        internal static Entity DivisionPreparingRules(Entity x) => x switch
        {
            Mulf(var any1, Divf(Integer(1), var any2)) => any1 / any2,
            Divf(Mulf(Number const1, var any1), var any2) => const1 * (any1 / any2),
            Mulf(Divf(Number const1, var any1), var any2) => const1 * (any2 / any1),
            _ => x
        };
        internal static Entity TrigonometricRules(Entity x) => x switch
        {
            // sin({}) * cos({}) = 1/2 * sin(2{})
            Mulf(Sinf(var any1), Cosf(var any1a)) when any1 == any1a => Rational.Create(1, 2) * new Sinf(2 * any1),
            Mulf(Cosf(var any1), Sinf(var any1a)) when any1 == any1a => Rational.Create(1, 2) * new Sinf(2 * any1),

            // arc1({}) + arc2({}) = pi/2
            Sumf(Arcsinf(var any1), Arccosf(var any1a)) when any1 == any1a => MathS.pi / 2,
            Sumf(Arccosf(var any1), Arcsinf(var any1a)) when any1 == any1a => MathS.pi / 2,
            Sumf(Arctanf(var any1), Arccotanf(var any1a)) when any1 == any1a => MathS.pi / 2,
            Sumf(Arccotanf(var any1), Arctanf(var any1a)) when any1 == any1a => MathS.pi / 2,

            // arcfunc(func(x)) = x
            Arcsinf(Sinf(var any1)) => any1,
            Arccosf(Cosf(var any1)) => any1,
            Arctanf(Tanf(var any1)) => any1,
            Arccotanf(Cotanf(var any1)) => any1,

            // func(arcfunc(x)) = x
            Sinf(Arcsinf(var any1)) => any1,
            Cosf(Arccosf(var any1)) => any1,
            Tanf(Arctanf(var any1)) => any1,
            Cotanf(Arccotanf(var any1)) => any1,

            // sin(:)^2 + cos(:)^2 = 1
            Sumf(Powf(Sinf(var any1), Integer(2)),
                 Powf(Cosf(var any1a), Integer(2))) when any1 == any1a => 1,
            Sumf(Powf(Cosf(var any1), Integer(2)),
                 Powf(Sinf(var any1a), Integer(2))) when any1 == any1a => 1,

            Minusf(Powf(Sinf(var any1), Integer(2)), Powf(Cosf(var any1a), Integer(2))) when any1 == any1a =>
                -1 * (new Powf(new Cosf(any1), 2) - new Powf(new Sinf(any1), 2)),
            Minusf(Powf(Cosf(var any1), Integer(2)), Powf(Sinf(var any1a), Integer(2))) when any1 == any1a =>
                new Cosf(2 * any1),
            _ => x
        };
        internal static Entity ExpandTrigonometricRules(Entity x) => x switch
        {
            Mulf(Rational(1, 2), Sinf(Mulf(Integer(2), var any1))) => new Sinf(any1) * new Cosf(any1),

            Cosf(Mulf(Integer(2), var any1)) =>
                new Powf(new Cosf(any1), Integer.Create(2)) - new Powf(new Sinf(any1), 2),

            _ => x
        };
        /// <summary>
        /// Here, we replace x with t which represents e^(ix).
        /// <list type="table">
        /// <item>sin(ax + b) = (t^a * e^(i*b) - t^(-a) * e^(-i*b)) / (2i)</item>
        /// <item>cos(ax + b) = (t^a * e^(i*b) + t^(-a) * e^(-i*b)) / 2</item>
        /// </list>
        /// </summary>
        internal static Func<Entity, Entity> TrigonometricToExponentialRules(Variable from, Variable to) => tree =>
        {
            // sin(ax + b) = (t^a * e^(i*b) - t^(-a) * e^(-i*b)) / (2i)
            Entity SinResult(Variable x, Number a, Entity b) =>
                x == from
                ? MathS.Pow(to, a) * (MathS.Pow(MathS.e, b * MathS.i) / (2 * MathS.i)) - MathS.Pow(to, -a) * MathS.Pow(MathS.e, -b * MathS.i) / (2 * MathS.i)
                : tree;
            // cos(ax + b) = (t^a * e^(i*b) + t^(-a) * e^(-i*b)) / 2
            Entity CosResult(Variable x, Number a, Entity b) =>
                x == from
                ? MathS.Pow(to, a) * (MathS.Pow(MathS.e, b * MathS.i) / 2) + MathS.Pow(to, -a) * MathS.Pow(MathS.e, -b * MathS.i) / 2
                : tree;
            // SolveLinear should also solve tan and cotan equations, but currently Polynomial solver cannot handle big powers
            // uncomment lines above when it will be fixed (TODO)
            // e.g. tan(ax + b) = -i + (2i)/(1 + e^(2i*b) t^(2a))
            return tree switch
            {
                Sinf(Variable x) => SinResult(x, 1, 0),
                Sinf(Mulf(Variable x, Number a)) => SinResult(x, a, 0),
                Sinf(Mulf(Number a, Variable x)) => SinResult(x, a, 0),
                Sinf(Sumf(Variable x, var b)) => SinResult(x, 1, b),
                Sinf(Sumf(var b, Variable x)) => SinResult(x, 1, b),
                Sinf(Sumf(Mulf(Variable x, Number a), var b)) => SinResult(x, a, b),
                Sinf(Sumf(Mulf(Number a, Variable x), var b)) => SinResult(x, a, b),
                Sinf(Sumf(var b, Mulf(Variable x, Number a))) => SinResult(x, a, b),
                Sinf(Sumf(var b, Mulf(Number a, Variable x))) => SinResult(x, a, b),
                Sinf(Minusf(Variable x, var b)) => SinResult(x, 1, -b),
                Sinf(Minusf(var b, Variable x)) => SinResult(x, -1, b),
                Sinf(Minusf(Mulf(Variable x, Number a), var b)) => SinResult(x, a, -b),
                Sinf(Minusf(Mulf(Number a, Variable x), var b)) => SinResult(x, a, -b),
                Sinf(Minusf(var b, Mulf(Variable x, Number a))) => SinResult(x, -a, b),
                Sinf(Minusf(var b, Mulf(Number a, Variable x))) => SinResult(x, -a, b),
                Cosf(Variable x) => CosResult(x, 1, 0),
                Cosf(Mulf(Variable x, Number a)) => CosResult(x, a, 0),
                Cosf(Mulf(Number a, Variable x)) => CosResult(x, a, 0),
                Cosf(Sumf(Variable x, var b)) => CosResult(x, 1, b),
                Cosf(Sumf(var b, Variable x)) => CosResult(x, 1, b),
                Cosf(Sumf(Mulf(Variable x, Number a), var b)) => CosResult(x, a, b),
                Cosf(Sumf(Mulf(Number a, Variable x), var b)) => CosResult(x, a, b),
                Cosf(Sumf(var b, Mulf(Variable x, Number a))) => CosResult(x, a, b),
                Cosf(Sumf(var b, Mulf(Number a, Variable x))) => CosResult(x, a, b),
                Cosf(Minusf(Variable x, var b)) => CosResult(x, 1, -b),
                Cosf(Minusf(var b, Variable x)) => CosResult(x, -1, b),
                Cosf(Minusf(Mulf(Variable x, Number a), var b)) => CosResult(x, a, -b),
                Cosf(Minusf(Mulf(Number a, Variable x), var b)) => CosResult(x, a, -b),
                Cosf(Minusf(var b, Mulf(Variable x, Number a))) => CosResult(x, -a, b),
                Cosf(Minusf(var b, Mulf(Number a, Variable x))) => CosResult(x, -a, b),
                _ => tree
            };
        };

        internal static Entity PowerRules(Entity x) => x switch
        {
            // {} ^ n * {}
            Mulf(Powf(var any1, var any2), var any1a) when any1 == any1a => new Powf(any1, any2 + 1),
            Mulf(var any1, Powf(var any1a, var any2)) when any1 == any1a => new Powf(any1, any2 + 1),

            // {} ^ n * {} ^ m = {} ^ (n + m)
            Mulf(Powf(var any1, var any2), Powf(var any1a, var any3)) when any1 == any1a => new Powf(any1, any2 + any3),

            // {} ^ n / {} ^ m = {} ^ (n - m)
            Divf(Powf(var any1, var any2), Powf(var any1a, var any3)) when any1 == any1a => new Powf(any1, any2 - any3),

            // ({} ^ {}) ^ {} = {} ^ ({} * {})
            Powf(Powf(var any1, var any2), var any3) => new Powf(any1, any2 * any3),

            // {1} ^ n * {2} ^ n = ({1} * {2}) ^ n
            Mulf(Powf(var any1, var any3), Powf(var any2, var any3a)) when any3 == any3a => new Powf(any1 * any2, any3),
            Divf(Powf(var any1, var any3), Powf(var any2, var any3a)) when any3 == any3a => new Powf(any1 / any2, any3),

            // x / x^n
            Divf(var any1, Powf(var any1a, var any2)) when any1 == any1a => new Powf(any1, 1 - any2),

            // x^n / x
            Divf(Powf(var any1, var any2), var any1a) when any1 == any1a => new Powf(any1, any2 - 1),

            // x^n / x^m
            Divf(Powf(var any1, var any2), Powf(var any1a, var any3)) when any1 == any1a => new Powf(any1, any2 - any3),

            // c ^ log(c, a) = a
            Powf(Number const1, Logf(Number const1a, var any1)) when const1 == const1a => any1,

            Mulf(Powf(var any1, var any3), Mulf(var any1a, var any2)) when any1 == any1a => new Powf(any1, any3 + 1) * any2,
            Mulf(Powf(var any1, var any3), Mulf(var any2, var any1a)) when any1 == any1a => new Powf(any1, any3 + 1) * any2,
            Mulf(Mulf(var any1, var any2), Powf(var any1a, var any3)) when any1 == any1a => new Powf(any1, any3 + 1) * any2,
            Mulf(Mulf(var any2, var any1), Powf(var any1a, var any3)) when any1 == any1a => new Powf(any1, any3 + 1) * any2,

            // (a * x) ^ c = a^c * x^c
            Powf(Mulf(Number const1, var any1), Number const2) =>
                new Powf(const1, const2) * new Powf(any1, const2),

            // {1} ^ (-1) = 1 / {1}
            Powf(var any1, Integer(-1)) => 1 / any1,

            // (a / {})^b * {} = a^b * {}^(1-b)
            Mulf(Powf(Divf(Number const1, var any1), Number const2), var any1a) when any1 == any1a =>
                new Powf(const1, const2) * new Powf(any1, 1 - const2),
            Mulf(Powf(Divf(Number const1, var any1), Number const2), Powf(var any1a, Number const3))
                when any1 == any1a => new Powf(const1, const2) * new Powf(any1, const3 - const2),

            // {1} / {2} / {2}
            Divf(Divf(var any1, var any2), var any2a) when any2 == any2a =>
                any1 / new Powf(any2, 2),
            Divf(Divf(var any1, Powf(var any2, var any3)), var any2a) when any2 == any2a =>
                any1 / new Powf(any2, any3 + 1),
            Divf(Divf(var any1, var any2), Powf(var any2a, var any3)) when any2 == any2a =>
                any1 / new Powf(any2, any3 + 1),
            Divf(Divf(var any1, Powf(var any2, var any4)), Powf(var any2a, var any3)) when any2 == any2a =>
                any1 / new Powf(any2, any3 + any4),

            // x * {} ^ {} = {} ^ {} * x
            Mulf(Variable var1, Powf(var any1, var any2)) => new Powf(any1, any2) * var1,

            _ => x
        };

        internal static Entity CommonRules(Entity x) => x switch
        {
            // (a * f(x)) * g(x) = a * (f(x) * g(x))
            Mulf(Mulf(Number const1, Function func1), Function func2) => func1 * func2 * const1,

            // (a/b) * (c/d) = (a*c)/(b*d)
            Mulf(Divf(var any1, var any2), Divf(var any3, var any4)) => any1 * any3 / (any2 * any4),

            // a / (b / c) = a * c / b
            Divf(var any1, Divf(var any2, var any3)) => any1 * any3 / any2,

            // a / b * c = a * c / b
            Mulf(Divf(var any1, var any2), var any3) => any1 * any3 / any2,

            // a * {1} / b
            Divf(Mulf(Number const1, var any1), Number const2) => const1 / const2 * any1,

            // a / b / c = a / (b * c)
            Divf(Divf(var any1, var any2), var any3) => any1 / (any2 * any3),

            // a * (b / c) = (a * b) / c
            Mulf(var any1, Divf(var any2, var any3)) => any1 * any2 / any3,

            // (a * f(x)) * b = (a * b) * f(x)
            Mulf(Mulf(Number const1, Function func1), Number const2) => const1 * const2 * func1,
            Mulf(Number const2, Mulf(Number const1, Function func1)) => const1 * const2 * func1,

            // (a * f(x)) * (b * g(x)) = (a * b) * (f(x) * g(x))
            Mulf(Mulf(Number const1, Function func1), Mulf(Number const2, Function func2)) =>
                func1 * func2 * (const1 * const2),

            // (f(x) + {}) + g(x) = (f(x) + g(x)) + {}
            Sumf(Sumf(Function func1, var any1), Function func2) => func1 + func2 + any1,

            // g(x) + (f(x) + {}) = (f(x) + g(x)) + {}
            Sumf(Function func2, Sumf(Function func1, var any1)) => func1 + func2 + any1,

            // x * a = a * x
            Mulf(Variable var1, Number const1) => const1 * var1,

            // a + x = x + a
            Sumf(Number const1, Variable var1) => var1 + const1,

            // f(x) * a = a * f(x)
            Mulf(Function func1, Number const1) => const1 * func1,

            // a + f(x) = f(x) + a
            Sumf(Number const1, Function func1) => func1 + const1,

            // a * x + b * x = (a + b) * x
            Sumf(Mulf(Number const1, Variable var1), Mulf(Number const2, Variable var1a))
                when var1 == var1a => (const1 + const2) * var1,

            // a * x - b * x = (a - b) * x
            Minusf(Mulf(Number const1, Variable var1), Mulf(Number const2, Variable var1a))
                when var1 == var1a => (const1 - const2) * var1,

            // {1} * {2} + {1} * {3} = {1} * ({2} + {3})
            Sumf(Mulf(var any1, var any2), Mulf(var any1a, var any3)) when any1 == any1a => any1 * (any2 + any3),
            Sumf(Mulf(var any2, var any1), Mulf(var any1a, var any3)) when any1 == any1a => any1 * (any2 + any3),
            Sumf(Mulf(var any1, var any2), Mulf(var any3, var any1a)) when any1 == any1a => any1 * (any2 + any3),
            Sumf(Mulf(var any2, var any1), Mulf(var any3, var any1a)) when any1 == any1a => any1 * (any2 + any3),
            Sumf(var any1, Mulf(var any1a, var any2)) when any1 == any1a => any1 * (1 + any2),
            Sumf(var any1, Mulf(var any2, var any1a)) when any1 == any1a => any1 * (1 + any2),
            Sumf(Mulf(var any1, var any2), var any1a) when any1 == any1a => any1 * (1 + any2),
            Sumf(Mulf(var any2, var any1), var any1a) when any1 == any1a => any1 * (1 + any2),
            Sumf(var any1, var any1a) when any1 == any1a => 2 * any1,

            Minusf(Mulf(var any1, var any2), Mulf(var any1a, var any3)) when any1 == any1a => any1 * (any2 - any3),
            Minusf(Mulf(var any2, var any1), Mulf(var any1a, var any3)) when any1 == any1a => any1 * (any2 - any3),
            Minusf(Mulf(var any1, var any2), Mulf(var any3, var any1a)) when any1 == any1a => any1 * (any2 - any3),
            Minusf(Mulf(var any2, var any1), Mulf(var any3, var any1a)) when any1 == any1a => any1 * (any2 - any3),
            Minusf(var any1, Mulf(var any1a, var any2)) when any1 == any1a => any1 * (1 - any2),
            Minusf(var any1, Mulf(var any2, var any1a)) when any1 == any1a => any1 * (1 - any2),
            Minusf(Mulf(var any1, var any2), var any1a) when any1 == any1a => any1 * (any2 - 1),
            Minusf(Mulf(var any2, var any1), var any1a) when any1 == any1a => any1 * (any2 - 1),
            Minusf(var any1, var any1a) when any1 == any1a => 0,

            // {1} / {2} + {1} * {3} = {1} * (1 / {2} + {3})
            Sumf(Divf(var any1, var any2), Mulf(var any1a, var any3)) when any1 == any1a => any1 * (1 / any2 + any3),
            Sumf(Divf(var any1, var any2), Mulf(var any3, var any1a)) when any1 == any1a => any1 * (1 / any2 + any3),
            Sumf(Mulf(var any2, var any1), Divf(var any1a, var any3)) when any1 == any1a => any1 * (any2 + 1 / any3),
            Sumf(Mulf(var any1, var any2), Divf(var any1a, var any3)) when any1 == any1a => any1 * (any2 + 1 / any3),
            Sumf(var anyButNot1 and not Integer(1), Divf(var anyButNot1a, var any2))
                when anyButNot1 == anyButNot1a => anyButNot1 * (1 + 1 / any2),
            Sumf(Divf(var anyButNot1 and not Integer(1), var any2), var anyButNot1a)
                when anyButNot1 == anyButNot1a => anyButNot1 * (1 + 1 / any2),

            // {1} * {2} - {1} * {3} = {1} * ({2} - {3})
            Minusf(Mulf(var any1, var any2), Mulf(var any1a, var any3)) when any1 == any1a => any1 * (any2 - any3),

            // x * x = x ^ 2
            Mulf(var any1, var any1a) when any1 == any1a => new Powf(any1, 2),

            // (a * x) * b
            Mulf(Mulf(Number const1, Variable var1), Number const2) => const1 * const2 * var1,
            Mulf(Number const2, Mulf(Number const1, Variable var1)) => const1 * const2 * var1,

            // (x + a) + b
            Sumf(Sumf(Variable var1, Number const1), Number const2) => var1 + (const1 + const2),

            // b + (x + a)
            Sumf(Number const2, Sumf(Variable var1, Number const1)) => var1 + (const1 + const2),

            // x * a * x
            Mulf(var any1, Mulf(var any1a, var any2)) when any1 == any1a => new Powf(any1, 2) * any2,
            Mulf(var any1, Mulf(var any2, var any1a)) when any1 == any1a => new Powf(any1, 2) * any2,
            Mulf(Mulf(var any1, var any2), var any1a) when any1 == any1a => new Powf(any1, 2) * any2,
            Mulf(Mulf(var any2, var any1), var any1a) when any1 == any1a => new Powf(any1, 2) * any2,

            // -1 * {1} + {2} = {2} - {1}
            Sumf(Mulf(Integer(-1), var any1), var any2) => any2 - any1,
            Sumf(var any1, Mulf(Integer(-1), var any2)) => any1 - any2,

            // (x - {}) (x + {}) = x2 - {}2
            Mulf(Minusf(Variable var1, var any1), Sumf(Variable var1a, var any1a))
                when (var1, any1) == (var1a, any1a) => new Powf(var1, 2) - new Powf(any1, 2),
            Mulf(Sumf(Variable var1, var any1), Minusf(Variable var1a, var any1a))
                when (var1, any1) == (var1a, any1a) => new Powf(var1, 2) - new Powf(any1, 2),

            // a / a
            Divf(var any1, var any1a) when any1 == any1a => 1,

            // (a * c) / c
            Divf(Mulf(var any1, var any2), var any2a) when any2 == any2a => any1,
            Divf(Mulf(var any2, var any1), var any2a) when any2 == any2a => any1,
            Divf(Mulf(var any1, var any2), Mulf(var any2a, var any3)) when any2 == any2a => any1 / any3,
            Divf(Mulf(var any1, var any2), Mulf(var any3, var any2a)) when any2 == any2a => any1 / any3,
            Divf(Mulf(var any2, var any1), Mulf(var any2a, var any3)) when any2 == any2a => any1 / any3,
            Divf(Mulf(var any2, var any1), Mulf(var any3, var any2a)) when any2 == any2a => any1 / any3,

            // ({1} - {2}) / ({2} - {1})
            Divf(Minusf(var any1, var any2), Minusf(var any2a, var any1a)) when (any1, any2) == (any1a, any2a) => -1,

            // ({1} + {2}) / ({2} + {1})
            Divf(Sumf(var any1, var any2), Sumf(var any2a, var any1a)) when (any1, any2) == (any1a, any2a) => 1,

            // a / (b * {1})
            Divf(Number const1, Mulf(Number const2, var any1)) => const1 / const2 / any1,
            Divf(Number const1, Mulf(var any1, Number const2)) => const1 / const2 / any1,

            // a * (b * {}) = (a * b) * {}
            Mulf(Number const1, Mulf(Number const2, var any1)) => const1 * const2 * any1,

            // {1} - {2} * {1}
            Minusf(var any1, Mulf(var any2, var any1a)) when any1 == any1a => any1 * (1 - any2),
            Minusf(var any1, Mulf(var any1a, var any2)) when any1 == any1a => any1 * (1 - any2),

            Sumf(var any1, Sumf(var any2, var any1a)) when any1 == any1a => 2 * any1 + any2,
            Sumf(var any1, Sumf(var any1a, var any2)) when any1 == any1a => 2 * any1 + any2,
            Sumf(Sumf(var any2, var any1), var any1a) when any1 == any1a => 2 * any1 + any2,
            Sumf(Sumf(var any1, var any2), var any1a) when any1 == any1a => 2 * any1 + any2,

            Minusf(Sumf(var any1, var any2), var any1a) when any1 == any1a => any2,
            Minusf(Sumf(var any2, var any1), var any1a) when any1 == any1a => any2,
            Minusf(var any1, Sumf(var any2, var any1a)) when any1 == any1a => -any2,
            Minusf(var any1, Sumf(var any1a, var any2)) when any1 == any1a => -any2,

            Sumf(var any1, Minusf(var any2, var any1a)) when any1 == any1a => any2,
            Sumf(var any1, Minusf(var any1a, var any2)) when any1 == any1a => 2 * any1 - any2,
            Sumf(Minusf(var any2, var any1), var any1a) when any1 == any1a => any2,
            Sumf(Minusf(var any1, var any2), var any1a) when any1 == any1a => 2 * any1 - any2,

            Minusf(var any1, Minusf(var any2, var any1a)) when any1 == any1a => 2 * any1 - any2,
            Minusf(var any1, Minusf(var any1a, var any2)) when any1 == any1a => any2,
            Minusf(Minusf(var any2, var any1), var any1a) when any1 == any1a => any2 - 2 * any1,
            Minusf(Minusf(var any1, var any2), var any1a) when any1 == any1a => -any2,

            Signumf(Signumf(var any1)) => new Signumf(any1),
            Absf(Absf(var any1)) => new Absf(any1),
            Absf(Signumf(_)) => 1,
            Signumf(Absf(_)) => 1,

            Mulf(Absf(var any1), Absf(var any2)) => new Absf(any1 * any2),
            Divf(Absf(var any1), Absf(var any2)) => new Absf(any1 / any2),
            Mulf(Signumf(var any1), Absf(var any1a)) when any1 == any1a => any1,
            Mulf(Absf(var any1a), Signumf(var any1)) when any1 == any1a => any1,

            _ => x
        };

        internal static Entity ExpandRules(Entity x) => x switch
        {
            Sinf(Sumf(var any1, var any2)) => new Sinf(any1) * new Cosf(any2) + new Sinf(any2) * new Cosf(any1),
            Sinf(Minusf(var any1, var any2)) => new Sinf(any1) * new Cosf(any2) - new Sinf(any2) * new Cosf(any1),

            _ => x
        };

        internal static Entity FactorizeRules(Entity x) => x switch
        {
            // {1}2 - {2}2
            Minusf(Powf(var any1, Number const1), Powf(var any2, Number const2)) =>
                (new Powf(any1, const1 / 2) - new Powf(any2, const2 / 2)) *
                (new Powf(any1, const1 / 2) + new Powf(any2, const2 / 2)),

            Minusf(Powf(var any1, Integer(2)), Number const1) =>
                (any1 - new Powf(const1, Rational.Create(1, 2))) *
                (any1 + new Powf(const1, Rational.Create(1, 2))),

            // {1} * {2} + {1} * {3} = {1} * ({2} + {3})
            Sumf(Mulf(var any1, var any2), Mulf(var any1a, var any3)) when any1 == any1a => any1 * (any2 + any3),
            Sumf(Mulf(var any1, var any2), Mulf(var any3, var any1a)) when any1 == any1a => any1 * (any2 + any3),
            Sumf(Mulf(var any2, var any1), Mulf(var any1a, var any3)) when any1 == any1a => any1 * (any2 + any3),
            Sumf(Mulf(var any2, var any1), Mulf(var any3, var any1a)) when any1 == any1a => any1 * (any2 + any3),
            Sumf(var any1, Mulf(var any1a, var any2)) when any1 == any1a => any1 * (1 + any2),
            Sumf(var any1, Mulf(var any2, var any1a)) when any1 == any1a => any1 * (1 + any2),
            Sumf(Mulf(var any1, var any2), var any1a) when any1 == any1a => any1 * (1 + any2),
            Sumf(Mulf(var any2, var any1), var any1a) when any1 == any1a => any1 * (1 + any2),
            Sumf(var any1, var any1a) when any1 == any1a => 2 * any1,

            Minusf(Mulf(var any1, var any2), Mulf(var any1a, var any3)) when any1 == any1a => any1 * (any2 - any3),
            Minusf(Mulf(var any2, var any1), Mulf(var any1a, var any3)) when any1 == any1a => any1 * (any2 - any3),
            Minusf(Mulf(var any1, var any2), Mulf(var any3, var any1a)) when any1 == any1a => any1 * (any2 - any3),
            Minusf(Mulf(var any2, var any1), Mulf(var any3, var any1a)) when any1 == any1a => any1 * (any2 - any3),
            Minusf(var any1, Mulf(var any1a, var any2)) when any1 == any1a => any1 * (1 - any2),
            Minusf(var any1, Mulf(var any2, var any1a)) when any1 == any1a => any1 * (1 - any2),
            Minusf(Mulf(var any1, var any2), var any1a) when any1 == any1a => any1 * (any2 - 1),
            Minusf(Mulf(var any2, var any1), var any1a) when any1 == any1a => any1 * (any2 - 1),
            Minusf(var any1, var any1a) when any1 == any1a => 0,

            // a ^ b * c ^ b = (a * c) ^ b
            Mulf(Powf(var any1, var any2), Powf(var any3, var any2a)) when any2 == any2a => new Powf(any1 * any3, any2),

            _ => x
        };


        private static bool IsLogic(Entity a)
            => a is Statement or Variable;

        private static bool IsLogic(Entity a, Entity b)
            => IsLogic(a) && IsLogic(b);

        private static bool IsLogic(Entity a, Entity b, Entity c)
            => IsLogic(a, b) && IsLogic(c);

        internal static Entity BooleanRules(Entity x) => x switch
        {
            Impliesf(var ass, _) when ass == False && IsLogic(ass) => True,
            Andf(Notf(var any1), Notf(var any2)) when IsLogic(any1, any2) => !(any1 | any2),
            Orf(Notf(var any1), Notf(var any2)) when IsLogic(any1, any2) => !(any1 & any2),
            Orf(Notf(var any1), var any1a) when any1 == any1a && IsLogic(any1) => True,
            Orf(Notf(var any1), var any2) when IsLogic(any1, any2) => any1.Implies(any2),
            Andf(var any1, var any1a) when any1 == any1a && IsLogic(any1) => any1,
            Orf(var any1, var any1a) when any1 == any1a && IsLogic(any1) => any1,
            Impliesf(var any1, var any1a) when any1 == any1a && IsLogic(any1) => True,
            Xorf(var any1, var any1a) when any1 == any1a && IsLogic(any1) => False,
            Notf(Notf(var any1)) when IsLogic(any1) => any1,

            Orf(Andf(var any1, var any2), Andf(var any1a, var any3)) when any1 == any1a && IsLogic(any1, any2, any3) => any1 & (any2 | any3),
            Andf(Orf(var any1, var any2), Orf(var any1a, var any3))  when any1 == any1a && IsLogic(any1, any2, any3) => any1 | (any2 & any3),
            Orf(Andf(var any1, var any2), Andf(var any3, var any1a)) when any1 == any1a && IsLogic(any1, any2, any3) => any1 & (any2 | any3),
            Andf(Orf(var any1, var any2), Orf(var any3, var any1a))  when any1 == any1a && IsLogic(any1, any2, any3) => any1 | (any2 & any3),
            Orf(Andf(var any2, var any1), Andf(var any1a, var any3)) when any1 == any1a && IsLogic(any1, any2, any3) => any1 & (any2 | any3),
            Andf(Orf(var any2, var any1), Orf(var any1a, var any3))  when any1 == any1a && IsLogic(any1, any2, any3) => any1 | (any2 & any3),
            Orf(Andf(var any2, var any1), Andf(var any3, var any1a)) when any1 == any1a && IsLogic(any1, any2, any3) => any1 & (any2 | any3),
            Andf(Orf(var any2, var any1), Orf(var any3, var any1a))  when any1 == any1a && IsLogic(any1, any2, any3) => any1 | (any2 & any3),

            Orf(var any1, Andf(var any1a, _)) when any1 == any1a && IsLogic(any1) => any1,
            Andf(var any1, Orf(var any1a, _)) when any1 == any1a && IsLogic(any1) => any1,

            Orf(var any1, Andf(Notf(var any1a), var any2)) when any1 == any1a && IsLogic(any1) => any1 | any2,
            Andf(var any1, Orf(Notf(var any1a), var any2)) when any1 == any1a && IsLogic(any1) => any1 & any2,
            Orf(var any1, Andf(var any1a, Notf(var any2))) when any1 == any1a && IsLogic(any1) => any1 | any2,
            Andf(var any1, Orf(var any1a, Notf(var any2))) when any1 == any1a && IsLogic(any1) => any1 & any2,
            Orf(Andf(Notf(var any1a), var any2), var any1) when any1 == any1a && IsLogic(any1) => any1 | any2,
            Andf(Orf(Notf(var any1a), var any2), var any1) when any1 == any1a && IsLogic(any1) => any1 & any2,
            Orf(Andf(var any1a, Notf(var any2)), var any1) when any1 == any1a && IsLogic(any1) => any1 | any2,
            Andf(Orf(var any1a, Notf(var any2)), var any1) when any1 == any1a && IsLogic(any1) => any1 & any2,

            Impliesf(Notf(var any1), Notf(var any2)) when IsLogic(any1, any2) => any2.Implies(any1),

            _ => x
        };

        /// <summary>Actual sorting with <see cref="Entity.SortHash(TreeAnalyzer.SortLevel)"/></summary>
        internal static Func<Entity, Entity> SortRules(Functions.TreeAnalyzer.SortLevel level) => x => x switch
        {
            Sumf or Minusf =>
                Sumf.LinearChildren(x).OrderBy(e => e.SortHash(level)).Aggregate((a, b) => new Sumf(a, b)),
            Mulf or Divf =>
                Mulf.LinearChildren(x).OrderBy(e => e.SortHash(level)).Aggregate((a, b) => new Mulf(a, b)),
            _ => x,
        };
        internal static Entity PolynomialLongDivision(Entity x) =>
            x is Divf(var num, var denom)
            && TreeAnalyzer.PolynomialLongDivision(num, denom) is var (divided, remainder)
            ? divided + remainder
            : x;
    }
}