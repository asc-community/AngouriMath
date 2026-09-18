//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Core.Sets
{
    internal static partial class SetOperators
    {
        /// <summary>
        /// Where a special set sits in the chain <c>ZZ+ ⊂ ZZ* ⊂ ZZ ⊂ QQ ⊂ RR ⊂ CC</c>, or
        /// <see langword="null"/> for <c>BB</c>, which is beside the chain rather than on it.
        /// https://github.com/asc-community/AngouriMath/issues/1409
        /// </summary>
        private static int? RankInTheNumberChain(SpecialSet set)
            => set switch
            {
                SpecialSet.PositiveIntegers => 0,
                SpecialSet.NonNegativeIntegers => 1,
                SpecialSet.Integers => 2,
                SpecialSet.Rationals => 3,
                SpecialSet.Reals => 4,
                SpecialSet.Complexes => 5,
                _ => null
            };

        /// <summary>Two nested sets unite to the larger; <c>BB</c> with a number set has no name.</summary>
        internal static Set? UniteSpecialSets(SpecialSet a, SpecialSet b)
        {
            if (a == b)
                return a;
            if (RankInTheNumberChain(a) is not { } rankA || RankInTheNumberChain(b) is not { } rankB)
                return null;
            return rankA > rankB ? a : b;
        }

        /// <summary>Two nested sets intersect to the smaller; <c>BB</c> shares nothing with a number set.</summary>
        internal static Set IntersectSpecialSets(SpecialSet a, SpecialSet b)
        {
            if (a == b)
                return a;
            if (RankInTheNumberChain(a) is not { } rankA || RankInTheNumberChain(b) is not { } rankB)
                return Empty;
            return rankA < rankB ? a : b;
        }

        /// <summary>
        /// The difference of two special sets: empty when the first is inside the second, the
        /// first when the two are disjoint, the one-element and the signed remainders by name
        /// where there is a name, and the set-builder set <c>{ x in A : not x in B }</c> where
        /// there is not — which is the set, rather than the difference left as written.
        /// </summary>
        internal static Set SetSubtractSpecialSets(SpecialSet a, SpecialSet b)
        {
            if (a == b)
                return Empty;
            if (RankInTheNumberChain(a) is not { } rankA || RankInTheNumberChain(b) is not { } rankB)
                return a;
            if (rankA < rankB)
                return Empty;
            // Built as the parser builds `{ x in A : p }`, with the membership as the bound name,
            // so that the difference prints and reads back in that form.
            var x = Variable.CreateVariableOrConstant("x");
            return (a, b) switch
            {
                (SpecialSet.NonNegativeIntegers, SpecialSet.PositiveIntegers) => new FiniteSet(Number.Integer.Zero),
                (SpecialSet.Integers, SpecialSet.NonNegativeIntegers) => new ConditionalSet(x.In(a), x < 0),
                (SpecialSet.Integers, SpecialSet.PositiveIntegers) => new ConditionalSet(x.In(a), x <= 0),
                _ => new ConditionalSet(x.In(a), !x.In(b))
            };
        }
    }
}
