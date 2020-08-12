
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
using AngouriMath.Core.Numerix;
using AngouriMath.Core.TreeAnalysis;

namespace AngouriMath
{
    internal static class Patterns
    {
        // I really want variable declarations under or patterns: https://github.com/dotnet/csharplang/issues/3740
        // Currently they fail with error CS8780: A variable may not be declared within a 'not' or 'or' pattern.
        /// <summary>a ^ (-1) => 1 / a</summary>
        internal static Entity InvertNegativePowers(Entity expr) =>
            expr is Powf(var @base, IntegerNumber { IsNegative: true } pow)
            ? 1 / MathS.Pow(@base, -1 * pow)
            : expr;
        /// <summary>1 + (-x) => 1 - x</summary>
        internal static Entity InvertNegativeMultipliers(Entity expr) =>
            expr is Sumf(var any1, Mulf(RealNumber { IsNegative: true } const1, var any2))
            ? any1 - (-1 * const1) * any2
            : expr;
        /// <summary>(x + a)! / (x + b)! -> (x+b+1)*(x+b+2)*...*(x+a)</summary>
        internal static Entity ExpandFactorialDivisions(Entity expr)
        {
            Entity ExpandFactorialDivisions(Entity x, Entity x2, NumberEntity num, NumberEntity den)
            {
                static Entity Add(Entity a, NumberEntity b) =>
                    b is IntegerNumber(0) ? a : a + b;
                if (x == x2
                    && num - den is IntegerNumber { Integer: var diff }
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
                Divf(Factorialf(Sumf(var any1, NumberEntity const1)), Factorialf(Sumf(var any1a, NumberEntity const2)))
                    => ExpandFactorialDivisions(any1, any1a, const1, const2),
                Divf(Factorialf(var any1), Factorialf(Sumf(var any1a, NumberEntity const2)))
                    => ExpandFactorialDivisions(any1, any1a, 0, const2),
                Divf(Factorialf(Sumf(var any1, NumberEntity const1)), Factorialf(var any1a))
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
        internal static Entity CollapseFactorialMultiplications(Entity expr)
        {
            Entity CollapseFactorialMultiplications(Entity x, Entity x2, NumberEntity factConst, NumberEntity @const) =>
                x == x2 && factConst + 1 == @const ? new Factorialf(x) : expr;
            return expr switch
            {
                Mulf(Factorialf(Sumf(var any1, NumberEntity const1)), Sumf(var any1a, NumberEntity const2)) =>
                    CollapseFactorialMultiplications(any1, any1a, const1, const2),
                Mulf(Factorialf(var any1), Sumf(var any1a, NumberEntity const2)) =>
                    CollapseFactorialMultiplications(any1, any1a, 0, const2),
                Mulf(Factorialf(Sumf(var any1, NumberEntity const1)), var any1a) =>
                    CollapseFactorialMultiplications(any1, any1a, const1, 0),
                _ => expr
            };
        }
        internal static Entity DivisionPreparingRules(Entity x) => x switch
        {
            Mulf(var any1, Divf(IntegerNumber(1), var any2)) => any1 / any2,
            Divf(Mulf(NumberEntity const1, var any1), var any2) => const1 * (any1 / any2),
            Mulf(Divf(NumberEntity const1, var any1), var any2) => const1 * (any2 / any1),
            _ => x
        };
        internal static Entity TrigonometricRules(Entity x) => x switch
        {
            // sin({}) * cos({}) = 1/2 * sin(2{})
            Mulf(Sinf(var any1), Cosf(var any1a)) when any1 == any1a =>
                RationalNumber.Create(1, 2) * new Sinf(2 * any1),
            Mulf(Cosf(var any1), Sinf(var any1a)) when any1 == any1a =>
                RationalNumber.Create(1, 2) * new Sinf(2 * any1),

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
            Sumf(Powf(Sinf(var any1), IntegerNumber(2)),
                 Powf(Cosf(var any1a), IntegerNumber(2))) when any1 == any1a => 1,
            Sumf(Powf(Cosf(var any1), IntegerNumber(2)),
                 Powf(Sinf(var any1a), IntegerNumber(2))) when any1 == any1a => 1,

            Minusf(Powf(Sinf(var any1), IntegerNumber(2)), Powf(Cosf(var any1a), IntegerNumber(2))) when any1 == any1a =>
                -1 * (new Powf(new Cosf(any1), 2) - new Powf(new Sinf(any1), 2)),
            Minusf(Powf(Cosf(var any1), IntegerNumber(2)), Powf(Sinf(var any1a), IntegerNumber(2))) when any1 == any1a =>
                new Cosf(2 * any1),
            _ => x
        };
        internal static Entity ExpandTrigonometricRules(Entity x) => x switch
        {
            Mulf(RationalNumber(1, 2), Sinf(Mulf(IntegerNumber(2), var any1))) => new Sinf(any1) * new Cosf(any1),

            Cosf(Mulf(IntegerNumber(2), var any1)) =>
                new Powf(new Cosf(any1), IntegerNumber.Create(2)) - new Powf(new Sinf(any1), 2),

            _ => x
        };

        internal static Entity PowerRules(Entity x) => x switch
        {
            // x * {} ^ {} = {} ^ {} * x
            Mulf(VariableEntity var1, Powf(var any1, var any2)) => new Powf(any1, any2) * var1,

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
            Powf(NumberEntity const1, Logf(NumberEntity const1a, var any1)) when const1 == const1a => any1,

            Mulf(Powf(var any1, var any3), Mulf(var any1a, var any2)) when any1 == any1a => new Powf(any1, any3 + 1) * any2,
            Mulf(Powf(var any1, var any3), Mulf(var any2, var any1a)) when any1 == any1a => new Powf(any1, any3 + 1) * any2,
            Mulf(Mulf(var any1, var any2), Powf(var any1a, var any3)) when any1 == any1a => new Powf(any1, any3 + 1) * any2,
            Mulf(Mulf(var any2, var any1), Powf(var any1a, var any3)) when any1 == any1a => new Powf(any1, any3 + 1) * any2,

            // (a * x) ^ c = a^c * x^c
            Powf(Mulf(NumberEntity const1, var any1), NumberEntity const2) =>
                new Powf(const1, const2) * new Powf(any1, const2),

            // {1} ^ (-1) = 1 / {1}
            Powf(var any1, IntegerNumber(-1)) => 1 / any1,

            // (a / {})^b * {} = a^b * {}^(1-b)
            Mulf(Powf(Divf(NumberEntity const1, var any1), NumberEntity const2), var any1a) when any1 == any1a =>
                new Powf(const1, const2) * new Powf(any1, 1 - const2),
            Mulf(Powf(Divf(NumberEntity const1, var any1), NumberEntity const2), Powf(var any1a, NumberEntity const3))
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

            _ => x
        };

        internal static Entity CommonRules(Entity x) => x switch
        {
            // (a * f(x)) * g(x) = a * (f(x) * g(x))
            Mulf(Mulf(NumberEntity const1, FunctionEntity func1), FunctionEntity func2) => func1 * func2 * const1,

            // a / (b / c) = a * c / b
            Divf(var any1, Divf(var any2, var any3)) => any1 * any3 / any2,

            // a / b / c = a / (b * c)
            Divf(Divf(var any1, var any2), var any3) => any1 / (any2 * any3),

            // a * (b / c) = (a * b) / c
            Mulf(var any1, Divf(var any2, var any3)) => any1 * any2 / any3,

            // (a * f(x)) * b = (a * b) * f(x)
            Mulf(Mulf(NumberEntity const1, FunctionEntity func1), NumberEntity const2) => const1 * const2 * func1,
            Mulf(NumberEntity const2, Mulf(NumberEntity const1, FunctionEntity func1)) => const1 * const2 * func1,

            // (a * f(x)) * (b * g(x)) = (a * b) * (f(x) * g(x))
            Mulf(Mulf(NumberEntity const1, FunctionEntity func1), Mulf(NumberEntity const2, FunctionEntity func2)) =>
                func1 * func2 * (const1 * const2),

            // (f(x) + {}) + g(x) = (f(x) + g(x)) + {}
            Sumf(Sumf(FunctionEntity func1, var any1), FunctionEntity func2) => func1 + func2 + any1,

            // g(x) + (f(x) + {}) = (f(x) + g(x)) + {}
            Sumf(FunctionEntity func2, Sumf(FunctionEntity func1, var any1)) => func1 + func2 + any1,

            // x * a = a * x
            Mulf(VariableEntity var1, NumberEntity const1) => const1 * var1,

            // a + x = x + a
            Sumf(NumberEntity const1, VariableEntity var1) => var1 + const1,

            // f(x) * a = a * f(x)
            Mulf(FunctionEntity func1, NumberEntity const1) => const1 * func1,

            // a + f(x) = f(x) + a
            Sumf(NumberEntity const1, FunctionEntity func1) => func1 + const1,

            // a * x + b * x = (a + b) * x
            Sumf(Mulf(NumberEntity const1, VariableEntity var1), Mulf(NumberEntity const2, VariableEntity var1a))
                when var1 == var1a => (const1 + const2) * var1,

            // a * x - b * x = (a - b) * x
            Minusf(Mulf(NumberEntity const1, VariableEntity var1), Mulf(NumberEntity const2, VariableEntity var1a))
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
            Sumf(var anyButNot1, Divf(var anyButNot1a, var any2))
                when anyButNot1 == anyButNot1a && anyButNot1 is not IntegerNumber(1) => anyButNot1 * (1 + 1 / any2),
            Sumf(Divf(var anyButNot1, var any2), var anyButNot1a)
                when anyButNot1 == anyButNot1a && anyButNot1 is not IntegerNumber(1) => anyButNot1 * (1 + 1 / any2),

            // {1} * {2} - {1} * {3} = {1} * ({2} - {3})
            Minusf(Mulf(var any1, var any2), Mulf(var any1a, var any3)) when any1 == any1a => any1 * (any2 - any3),

            // x * x = x ^ 2
            Mulf(var any1, var any1a) when any1 == any1a => new Powf(any1, 2),

            // (a * x) * b
            Mulf(Mulf(NumberEntity const1, VariableEntity var1), NumberEntity const2) => const1 * const2 * var1,
            Mulf(NumberEntity const2, Mulf(NumberEntity const1, VariableEntity var1)) => const1 * const2 * var1,

            // (x + a) + b
            Sumf(Sumf(VariableEntity var1, NumberEntity const1), NumberEntity const2) => var1 + (const1 + const2),

            // b + (x + a)
            Sumf(NumberEntity const2, Sumf(VariableEntity var1, NumberEntity const1)) => var1 + (const1 + const2),

            // x * a * x
            Mulf(var any1, Mulf(var any1a, var any2)) when any1 == any1a => new Powf(any1, 2) * any2,
            Mulf(var any1, Mulf(var any2, var any1a)) when any1 == any1a => new Powf(any1, 2) * any2,
            Mulf(Mulf(var any1, var any2), var any1a) when any1 == any1a => new Powf(any1, 2) * any2,
            Mulf(Mulf(var any2, var any1), var any1a) when any1 == any1a => new Powf(any1, 2) * any2,

            // -1 * {1} + {2} = {2} - {1}
            Sumf(Mulf(IntegerNumber(-1), var any1), var any2) => any2 - any1,
            Sumf(var any1, Mulf(IntegerNumber(-1), var any2)) => any1 - any2,

            // (x - {}) (x + {}) = x2 - {}2
            Mulf(Minusf(VariableEntity var1, var any1), Sumf(VariableEntity var1a, var any1a))
                when var1 == var1a && any1 == any1a => new Powf(var1, 2) - new Powf(any1, 2),
            Mulf(Sumf(VariableEntity var1, var any1), Minusf(VariableEntity var1a, var any1a))
                when var1 == var1a && any1 == any1a => new Powf(var1, 2) - new Powf(any1, 2),

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
            Divf(NumberEntity const1, Mulf(NumberEntity const2, var any1)) => const1 / const2 / any1,
            Divf(NumberEntity const1, Mulf(var any1, NumberEntity const2)) => const1 / const2 / any1,

            // a * (b * {}) = (a * b) * {}
            Mulf(NumberEntity const1, Mulf(NumberEntity const2, var any1)) => const1 * const2 * any1,

            // {1} - {2} * {1}
            Minusf(var any1, Mulf(var any2, var any1a)) when any1 == any1a => any1 * (1 - any2),
            Minusf(var any1, Mulf(var any1a, var any2)) when any1 == any1a => any1 * (1 - any2),

            // a / {} * b
            Mulf(Divf(var any1, var any2), var any3) => any1 * any3 / any2,

            // a * {1} / b
            Divf(Mulf(NumberEntity const1, var any1), NumberEntity const2) => const1 / const2 * any1,

            _ => x
        };

        internal static Entity ExpandRules(Entity x) => x switch
        {
            Sinf(Sumf(var any1, var any2)) => new Sinf(any1) * new Cosf(any2) + new Sinf(any2) * new Cosf(any1),
            Sinf(Minusf(var any1, var any2)) => new Sinf(any1) * new Cosf(any2) - new Sinf(any2) * new Cosf(any1),

            _ => x
        };

        internal static Entity CollapseRules(Entity x) => x switch
        {
            // {1}2 - {2}2
            Minusf(Powf(var any1, NumberEntity const1), Powf(var any2, NumberEntity const2)) =>
                (new Powf(any1, const1 / 2) - new Powf(any2, const2 / 2)) *
                (new Powf(any1, const1 / 2) + new Powf(any2, const2 / 2)),

            Minusf(Powf(var any1, IntegerNumber(2)), NumberEntity const1) =>
                (any1 - new Powf(const1, RationalNumber.Create(1, 2))) *
                (any1 + new Powf(const1, RationalNumber.Create(1, 2))),

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
        /// <summary>Actual sorting with <see cref="Entity.SortHash(TreeAnalyzer.SortLevel)"/></summary>
        internal static Func<Entity, Entity> SortRules(TreeAnalyzer.SortLevel level) => tree =>
        {
            switch (tree)
            {
                case Sumf or Minusf:
                    var linChildren = Sumf.LinearChildren(tree);
                    var groups = TreeAnalyzer.GroupByHash(linChildren, level);
                    var grouppedChildren =
                        groups.Select(list => TreeAnalyzer.MultiHangLinear(list, (a, b) => new Sumf(a, b))).ToList();
                    return TreeAnalyzer.MultiHangLinear(grouppedChildren, (a, b) => new Sumf(a, b));
                case Mulf or Divf:
                    linChildren = Mulf.LinearChildren(tree);
                    groups = TreeAnalyzer.GroupByHash(linChildren, level);
                    grouppedChildren =
                        groups.Select(list => TreeAnalyzer.MultiHangLinear(list, (a, b) => new Mulf(a, b))).ToList();
                    return TreeAnalyzer.MultiHangLinear(grouppedChildren, (a, b) => new Mulf(a, b));
                default:
                    return tree;
            }
        };
        internal static Entity OptimizeRules(Entity x) => x switch
        {
            Sumf or Minusf => TreeAnalyzer.MultiHangBinary(Sumf.LinearChildren(x).ToList(), (a, b) => new Sumf(a, b)),
            Mulf or Divf => TreeAnalyzer.MultiHangBinary(Mulf.LinearChildren(x).ToList(), (a, b) => new Mulf(a, b)),
            _ => x
        };
    }
}
