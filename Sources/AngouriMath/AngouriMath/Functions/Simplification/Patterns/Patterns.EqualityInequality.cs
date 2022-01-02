//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using static AngouriMath.Entity;
using static AngouriMath.Entity.Boolean;

namespace AngouriMath.Functions
{
    internal static partial class Patterns
    {
        private static bool IsRealPositive(Entity entity)
            => entity is Real re && re > 0;

        private static bool IsRealNegative(Entity entity)
            => entity is Real re && re < 0;

        private static bool IsNonZero(Entity entity)
             => !IsZero(entity);

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

            // The following set of patterns might be simplified

            // 4 * a ? 0
            Equalsf        (Mulf(var rePo, var any1), var zeroEnt) when IsRealPositive(rePo) && IsZero(zeroEnt) => any1.Equalizes(Integer.Zero),
            Greaterf       (Mulf(var rePo, var any1), var zeroEnt) when IsRealPositive(rePo) && IsZero(zeroEnt) => any1 > Integer.Zero,
            GreaterOrEqualf(Mulf(var rePo, var any1), var zeroEnt) when IsRealPositive(rePo) && IsZero(zeroEnt) => any1 >= Integer.Zero,
            Lessf          (Mulf(var rePo, var any1), var zeroEnt) when IsRealPositive(rePo) && IsZero(zeroEnt) => any1 < Integer.Zero,
            LessOrEqualf   (Mulf(var rePo, var any1), var zeroEnt) when IsRealPositive(rePo) && IsZero(zeroEnt) => any1 <= Integer.Zero,

            // a * 4 ? 0
            Equalsf        (Mulf(var any1, var rePo), var zeroEnt) when IsRealPositive(rePo) && IsZero(zeroEnt) => any1.Equalizes(Integer.Zero),
            Greaterf       (Mulf(var any1, var rePo), var zeroEnt) when IsRealPositive(rePo) && IsZero(zeroEnt) => any1 > Integer.Zero,
            GreaterOrEqualf(Mulf(var any1, var rePo), var zeroEnt) when IsRealPositive(rePo) && IsZero(zeroEnt) => any1 >= Integer.Zero,
            Lessf          (Mulf(var any1, var rePo), var zeroEnt) when IsRealPositive(rePo) && IsZero(zeroEnt) => any1 < Integer.Zero,
            LessOrEqualf   (Mulf(var any1, var rePo), var zeroEnt) when IsRealPositive(rePo) && IsZero(zeroEnt) => any1 <= Integer.Zero,

            // -4 * a ? 0
            Equalsf        (Mulf(var rePo, var any1), var zeroEnt) when IsRealNegative(rePo) && IsZero(zeroEnt) => any1.Equalizes(Integer.Zero),
            Greaterf       (Mulf(var rePo, var any1), var zeroEnt) when IsRealNegative(rePo) && IsZero(zeroEnt) => any1 < Integer.Zero,
            GreaterOrEqualf(Mulf(var rePo, var any1), var zeroEnt) when IsRealNegative(rePo) && IsZero(zeroEnt) => any1 <= Integer.Zero,
            Lessf          (Mulf(var rePo, var any1), var zeroEnt) when IsRealNegative(rePo) && IsZero(zeroEnt) => any1 > Integer.Zero,
            LessOrEqualf   (Mulf(var rePo, var any1), var zeroEnt) when IsRealNegative(rePo) && IsZero(zeroEnt) => any1 >= Integer.Zero,

            // a * -4 ? 0
            Equalsf        (Mulf(var any1, var rePo), var zeroEnt) when IsRealNegative(rePo) && IsZero(zeroEnt) => any1.Equalizes(Integer.Zero),
            Greaterf       (Mulf(var any1, var rePo), var zeroEnt) when IsRealNegative(rePo) && IsZero(zeroEnt) => any1 < Integer.Zero,
            GreaterOrEqualf(Mulf(var any1, var rePo), var zeroEnt) when IsRealNegative(rePo) && IsZero(zeroEnt) => any1 <= Integer.Zero,
            Lessf          (Mulf(var any1, var rePo), var zeroEnt) when IsRealNegative(rePo) && IsZero(zeroEnt) => any1 > Integer.Zero,
            LessOrEqualf   (Mulf(var any1, var rePo), var zeroEnt) when IsRealNegative(rePo) && IsZero(zeroEnt) => any1 >= Integer.Zero,

            // a / 4 ? 0
            Equalsf        (Divf(var any1, var rePo), var zeroEnt) when IsRealPositive(rePo) && IsZero(zeroEnt) => any1.Equalizes(Integer.Zero),
            Greaterf       (Divf(var any1, var rePo), var zeroEnt) when IsRealPositive(rePo) && IsZero(zeroEnt) => any1 > Integer.Zero,
            GreaterOrEqualf(Divf(var any1, var rePo), var zeroEnt) when IsRealPositive(rePo) && IsZero(zeroEnt) => any1 >= Integer.Zero,
            Lessf          (Divf(var any1, var rePo), var zeroEnt) when IsRealPositive(rePo) && IsZero(zeroEnt) => any1 < Integer.Zero,
            LessOrEqualf   (Divf(var any1, var rePo), var zeroEnt) when IsRealPositive(rePo) && IsZero(zeroEnt) => any1 <= Integer.Zero,

            // a / -4 ? 0
            Equalsf        (Divf(var any1, var rePo), var zeroEnt) when IsRealNegative(rePo) && IsZero(zeroEnt) => any1.Equalizes(Integer.Zero),
            Greaterf       (Divf(var any1, var rePo), var zeroEnt) when IsRealNegative(rePo) && IsZero(zeroEnt) => any1 < Integer.Zero,
            GreaterOrEqualf(Divf(var any1, var rePo), var zeroEnt) when IsRealNegative(rePo) && IsZero(zeroEnt) => any1 <= Integer.Zero,
            Lessf          (Divf(var any1, var rePo), var zeroEnt) when IsRealNegative(rePo) && IsZero(zeroEnt) => any1 > Integer.Zero,
            LessOrEqualf   (Divf(var any1, var rePo), var zeroEnt) when IsRealNegative(rePo) && IsZero(zeroEnt) => any1 >= Integer.Zero,

            Greaterf(var any1, var any1a) when any1 == any1a => false,
            Lessf(var any1, var any1a) when any1 == any1a => false,
            GreaterOrEqualf(var any1, var any1a) when any1 == any1a => true,
            LessOrEqualf(var any1, var any1a) when any1 == any1a => true,

            _ => x
        };
    }
}
