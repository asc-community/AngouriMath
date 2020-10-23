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
using System.Linq;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;
using static AngouriMath.Entity.Boolean;
using System.Collections.Generic;
using static AngouriMath.Entity.Set;
using AngouriMath.Core;

namespace AngouriMath.Functions
{
    internal static partial class Patterns
    {
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

        internal static Entity PowerRules(Entity x) => x switch
        {
            // {1}^({2} / log({3}, {1})) = {3}^{2}
            Powf(var any1, Divf(var any2, Logf(var any3, var any1a))) when any1 == any1a => new Powf(any3, any2),

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

            Logf(var any1, Powf(var any2, var any3)) => any3 * MathS.Log(any1, any2),

            _ => x
        };
    }
}
