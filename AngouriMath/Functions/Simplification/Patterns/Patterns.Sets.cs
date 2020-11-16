/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System.Linq;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;
using static AngouriMath.Entity.Boolean;
using static AngouriMath.Entity.Set;
using AngouriMath.Core;

namespace AngouriMath.Functions
{
    internal static partial class Patterns
    {
        [ConstantField] private static readonly FiniteSet FullBooleanSet = new FiniteSet(True, False);

        internal static Entity SetOperatorRules(Entity x) => x switch
        {
            Intersectionf(var any1, var any1a) when any1 == any1a => any1,
            Unionf(var any1, var any1a) when any1 == any1a => any1,
            SetMinusf(var any1, var any1a) when any1 == any1a => Empty,
            ConditionalSet(var var1, Inf(var var1a, var set)) when var1 == var1a => set,

            Inf(var var1, FiniteSet finite) when finite.Count == 1 => var1.Equalizes(finite.First()),
            Inf(not Set and not Tensor and var var, Interval(var left, var leftClosed, var right, var rightClosed)) => 
            Simplificator.ParaphraseInterval(var, left, leftClosed, right, rightClosed),

            FiniteSet potentialBB when potentialBB == FullBooleanSet => SpecialSet.Create(Domain.Boolean),
            Interval(var left, _, var right, _) interval when left == Real.NegativeInfinity && right == Real.PositiveInfinity => SpecialSet.Create(interval.Codomain),

            _ => x
        };
    }
}
