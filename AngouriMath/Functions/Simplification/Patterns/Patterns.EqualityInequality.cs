/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;
using static AngouriMath.Entity.Boolean;

namespace AngouriMath.Functions
{
    internal static partial class Patterns
    {
        private static bool IsRealPositive(Entity entity)
            => entity is Real re && re > 0;

        private static bool IsZero(Entity entity)
            => entity is Real re && Real.IsZero(re);

        // Suggestions to refactor this?
        private static bool OppositeSigns(ComparisonSign left, ComparisonSign right)
        {
            if (left is Lessf)
                return right is Greaterf or GreaterOrEqualf or Equalsf;
            if (left is LessOrEqualf)
                return right is Greaterf;
            if (left is Greaterf)
                return right is Lessf or LessOrEqualf or Equalsf;
            if (left is GreaterOrEqualf)
                return right is Lessf;
            if (left is Equalsf)
                return right is Lessf or Greaterf;
            return false;
        }

        internal static Entity InequalityEqualityRules(Entity x) => x switch
        {
            Orf(Lessf(var any1, var any2), Equalsf(var any1a, var any2a)) when any1 == any1a && any2 == any2a => any1 <= any2,
            Orf(Lessf(var any2, var any1), Equalsf(var any1a, var any2a)) when any1 == any1a && any2 == any2a => any1 <= any2,
            Orf(Greaterf(var any1, var any2), Equalsf(var any1a, var any2a)) when any1 == any1a && any2 == any2a => any1 >= any2,
            Orf(Greaterf(var any2, var any1), Equalsf(var any1a, var any2a)) when any1 == any1a && any2 == any2a => any1 >= any2,
            Orf(Equalsf(var any1a, var any2a), Lessf(var any1, var any2)) when any1 == any1a && any2 == any2a => any1 <= any2,
            Orf(Equalsf(var any1a, var any2a), Lessf(var any2, var any1)) when any1 == any1a && any2 == any2a => any1 <= any2,
            Orf(Equalsf(var any1a, var any2a), Greaterf(var any1, var any2)) when any1 == any1a && any2 == any2a => any1 >= any2,
            Orf(Equalsf(var any1a, var any2a), Greaterf(var any2, var any1)) when any1 == any1a && any2 == any2a => any1 >= any2,

            Notf(Greaterf(var any1, var any2)) => any1 <= any2,
            Notf(Lessf(var any1, var any2)) => any1 >= any2,
            Notf(GreaterOrEqualf(var any1, var any2)) => any1 < any2,
            Notf(LessOrEqualf(var any1, var any2)) => any1 > any2,

            Impliesf(Andf(Greaterf(var any1, var any2), Greaterf(var any2a, var any3)), Greaterf(var any1a, var any3a))
                when any1 == any1a && any2 == any2a && any3 == any3a => True,

            Impliesf(Andf(Lessf(var any1, var any2), Lessf(var any2a, var any3)), Lessf(var any1a, var any3a))
                when any1 == any1a && any2 == any2a && any3 == any3a => True,

            Equalsf(var zero, var anyButZero) when IsZero(zero) && !IsZero(anyButZero) => anyButZero.Equalizes(zero),
            Greaterf(var zero, var anyButZero) when IsZero(zero) && !IsZero(anyButZero) => anyButZero < zero,
            Lessf(var zero, var anyButZero) when IsZero(zero) && !IsZero(anyButZero) => anyButZero > zero,
            GreaterOrEqualf(var zero, var anyButZero) when IsZero(zero) && !IsZero(anyButZero) => anyButZero <= zero,
            LessOrEqualf(var zero, var anyButZero) when IsZero(zero) && !IsZero(anyButZero) => anyButZero >= zero,

            Equalsf(var @const, var anyButConst) when @const is Number && anyButConst is not Number => anyButConst.Equalizes(@const),
            Greaterf(var @const, var anyButConst) when @const is Number && anyButConst is not Number => anyButConst < @const,
            Lessf(var @const, var anyButConst) when @const is Number && anyButConst is not Number => anyButConst > @const,
            GreaterOrEqualf(var @const, var anyButConst) when @const is Number && anyButConst is not Number => anyButConst <= @const,
            LessOrEqualf(var @const, var anyButConst) when @const is Number && anyButConst is not Number => anyButConst >= @const,

            Andf(ComparisonSign left, ComparisonSign right) when
            left.DirectChildren[0] == right.DirectChildren[0] &&
            left.DirectChildren[1] == right.DirectChildren[1] &&
            OppositeSigns(left, right) => False,

            Equalsf(Powf(var any1, var rePo), var zero) when IsRealPositive(rePo) && IsZero(zero) => any1.Equalizes(zero),

            Equalsf(Mulf(var rePo, var any1), var zeroEnt) when IsRealPositive(rePo) && IsZero(zeroEnt) => any1.Equalizes(Integer.Zero),
            Greaterf(Mulf(var rePo, var any1), var zeroEnt) when IsRealPositive(rePo) && IsZero(zeroEnt) => any1 > Integer.Zero,
            GreaterOrEqualf(Mulf(var rePo, var any1), var zeroEnt) when IsRealPositive(rePo) && IsZero(zeroEnt) => any1 >= Integer.Zero,
            Lessf(Mulf(var rePo, var any1), var zeroEnt) when IsRealPositive(rePo) && IsZero(zeroEnt) => any1 < Integer.Zero,
            LessOrEqualf(Mulf(var rePo, var any1), var zeroEnt) when IsRealPositive(rePo) && IsZero(zeroEnt) => any1 <= Integer.Zero,

            Equalsf(Mulf(var any1, var rePo), var zeroEnt) when IsRealPositive(rePo) && IsZero(zeroEnt) => any1.Equalizes(Integer.Zero),
            Greaterf(Mulf(var any1, var rePo), var zeroEnt) when IsRealPositive(rePo) && IsZero(zeroEnt) => any1 > Integer.Zero,
            GreaterOrEqualf(Mulf(var any1, var rePo), var zeroEnt) when IsRealPositive(rePo) && IsZero(zeroEnt) => any1 >= Integer.Zero,
            Lessf(Mulf(var any1, var rePo), var zeroEnt) when IsRealPositive(rePo) && IsZero(zeroEnt) => any1 < Integer.Zero,
            LessOrEqualf(Mulf(var any1, var rePo), var zeroEnt) when IsRealPositive(rePo) && IsZero(zeroEnt) => any1 <= Integer.Zero,

            _ => x
        };
    }
}
