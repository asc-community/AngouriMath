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
            Entity SinResult(Variable x, Entity a, Entity b) =>
                x == from
                ? MathS.Pow(to, a) * (MathS.Pow(MathS.e, b * MathS.i) / (2 * MathS.i)) - MathS.Pow(to, -a) * MathS.Pow(MathS.e, -b * MathS.i) / (2 * MathS.i)
                : tree;
            // cos(ax + b) = (t^a * e^(i*b) + t^(-a) * e^(-i*b)) / 2
            Entity CosResult(Variable x, Entity a, Entity b) =>
                x == from
                ? MathS.Pow(to, a) * (MathS.Pow(MathS.e, b * MathS.i) / 2) + MathS.Pow(to, -a) * MathS.Pow(MathS.e, -b * MathS.i) / 2
                : tree;
            // SolveLinear should also solve tan and cotan equations, but currently Polynomial solver cannot handle big powers
            // uncomment lines above when it will be fixed (TODO)
            // e.g. tan(ax + b) = -i + (2i)/(1 + e^(2i*b) t^(2a))
            return tree switch
            {
                Sinf(var arg) => TreeAnalyzer.TryGetPolyLinear(arg, from, out var a, out var b) ?
                    SinResult(from, a.InnerSimplified, b.InnerSimplified) : tree,

                Cosf(var arg) => TreeAnalyzer.TryGetPolyLinear(arg, from, out var a, out var b) ?
                    CosResult(from, a.InnerSimplified, b.InnerSimplified) : tree,
                _ => tree
            };
        };
    }
}
