//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

namespace AngouriMath.Core.Transformations
{
    /// <summary>
    /// One rewrite that actually fired: which rule set did it, the subexpression it matched,
    /// and what it put there instead.
    /// </summary>
    /// <remarks>
    /// The subexpression, not the whole expression. A rewrite pass walks the tree bottom-up
    /// and rewrites nodes as it goes, so there is no moment at which a partly-rewritten
    /// whole expression exists to be photographed — reporting one would mean building it,
    /// and it would be a picture of something the engine never held.
    /// </remarks>
    public readonly struct RewriteStep
    {
        internal RewriteStep(RewriteRuleSet ruleSet, RewriteRule? rule, Entity before, Entity after)
            => (RuleSet, Rule, Before, After) = (ruleSet, rule, before, after);

        /// <summary>Which rule set rewrote it.</summary>
        public RewriteRuleSet RuleSet { get; }

        /// <summary>
        /// Which single rewrite in that set did it, where the set is addressable at that grain —
        /// see <see cref="RewriteRuleSet.Rules"/>. Null where it is not.
        /// </summary>
        /// <remarks>
        /// This is the grain
        /// <a href="https://github.com/asc-community/AngouriMath/issues/28">#28</a> asks for: a
        /// derivation that names the identity applied, rather than the group of identities it
        /// was filed under.
        /// </remarks>
        public RewriteRule? Rule { get; }

        /// <summary>The subexpression as it was matched.</summary>
        public Entity Before { get; }

        /// <summary>What replaced it. Never equal to <see cref="Before"/> — a rule set that changed nothing records nothing.</summary>
        public Entity After { get; }

        /// <summary>What the rule set claims about the rewrite. See <see cref="RewriteRuleSet.Relation"/>.</summary>
        public TransformationRelation Relation => RuleSet.Relation;

        /// <summary>How well justified that claim is. See <see cref="Soundness"/> on what a tier is and is not.</summary>
        public Soundness Soundness => RuleSet.Soundness;

        /// <inheritdoc/>
        public override string ToString()
            => Rule is null
                ? $"{RuleSet.Name}: {Before.Stringize()} -> {After.Stringize()}"
                : $"{RuleSet.Name}/{Rule.Name}: {Before.Stringize()} -> {After.Stringize()}";
    }
}
