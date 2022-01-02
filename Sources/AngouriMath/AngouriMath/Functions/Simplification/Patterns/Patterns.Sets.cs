//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using static AngouriMath.Entity;
using static AngouriMath.Entity.Boolean;
using static AngouriMath.Entity.Set;

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
            Inf(not Set and not Matrix and var var, Interval(var left, var leftClosed, var right, var rightClosed)) => 
            Simplificator.ParaphraseInterval(var, left, leftClosed, right, rightClosed),

            FiniteSet potentialBB when potentialBB == FullBooleanSet => SpecialSet.Create(Domain.Boolean),
            Interval(var left, _, var right, _) interval when left == Real.NegativeInfinity && right == Real.PositiveInfinity => SpecialSet.Create(interval.Codomain),

            _ => x
        };
    }
}
