//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

namespace AngouriMath.Core.Transformations
{
    /// <summary>
    /// Reading order for <see cref="RewriteRuleSet.ApplyOnce(Entity)"/>.
    /// </summary>
    /// <remarks>
    /// The simplifier applies a dozen rule sets in sequence, and written as calls the
    /// sequence reads inside out. This is the same operation with the expression in front,
    /// so that a pipeline still reads in the order it runs.
    /// </remarks>
    internal static class RewriteRuleSetExtensions
    {
        /// <summary>Applies <paramref name="ruleSet"/> once over every node of <paramref name="expression"/>.</summary>
        internal static Entity Rewrite(this Entity expression, RewriteRuleSet ruleSet)
            => ruleSet.ApplyOnce(expression);
    }
}
