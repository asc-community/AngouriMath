//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using static AngouriMath.Entity.Set;

namespace AngouriMath.Core.Sets
{
    internal static partial class SetOperators
    {
        internal static (ConditionalSet one, ConditionalSet another) MergeToOneVariable
            (ConditionalSet one, ConditionalSet another)
        {
            var stat1 = another.Predicate.Substitute(another.Var, one.Var);
            return (one, (ConditionalSet)another.New(one.Var, stat1));
        }
    }
}
