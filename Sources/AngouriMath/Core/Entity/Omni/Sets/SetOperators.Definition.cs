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
