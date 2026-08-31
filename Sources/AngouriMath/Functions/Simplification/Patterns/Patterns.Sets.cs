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
        /// <remarks>Internal so the data form of this set compares against the same value.</remarks>
        [ConstantField] internal static readonly FiniteSet FullBooleanSet = new FiniteSet(True, False);

        [AddressableRules]
        internal static Entity SetOperatorRules(Entity x) => x switch
        {
            Intersectionf(var any1, var any1a) when any1 == any1a => any1,

            // A /\ (B \/ C) = (A /\ B) \/ (A /\ C). Without this, an intersection whose
            // other side is a union is left as written however simple each piece is, which
            // is half of https://github.com/asc-community/AngouriMath/issues/415.
            Intersectionf(Set setA, Unionf(Set setB, Set setC)) =>
                setA.Intersect(setB).Unite(setA.Intersect(setC)),
            Intersectionf(Unionf(Set setB, Set setC), Set setA) =>
                setB.Intersect(setA).Unite(setC.Intersect(setA)),
            Unionf(var any1, var any1a) when any1 == any1a => any1,
            SetMinusf(var any1, var any1a) when any1 == any1a => Empty,
            ConditionalSet(var var1, Inf(var var1a, var set)) when var1 == var1a => set,

            Inf(var var1, FiniteSet finite) when finite.Count == 1 => var1.EqualTo(finite.First()),
            Inf(not Set and not Matrix and var var, Interval(var left, var leftClosed, var right, var rightClosed)) => 
            Simplificator.ParaphraseInterval(var, left, leftClosed, right, rightClosed),

            FiniteSet potentialBB when potentialBB == FullBooleanSet => SpecialSet.Create(Domain.Boolean),
            // (-oo; +oo) is the domain it is an interval of -- where that domain names a set.
            // An interval widened to Domain.Any is left as written: "no constraint" has no set,
            // and asking SpecialSet.Create for one threw out of `solve` on valid input.
            // https://github.com/asc-community/AngouriMath/issues/996
            Interval(var left, _, var right, _) interval
                when left == Real.NegativeInfinity && right == Real.PositiveInfinity
                     && interval.Codomain is not AngouriMath.Core.Domain.Any
                => SpecialSet.Create(interval.Codomain),

            _ => x
        };
    }
}
