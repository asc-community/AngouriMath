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

        /// <summary>
        /// How well justified that claim is — <b>this rewrite's own tier where it has one</b>, and
        /// its set's where it has not. See <see cref="Soundness"/> on what a tier is and is not.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This read the set's tier and nothing else until a rule could carry one, which made it
        /// the same answer for every step of a set — and a set's tier is the <i>minimum</i> over
        /// its rules, so one conditional rule spoke for a hundred. All thirty sets declare
        /// <see cref="Transformations.Soundness.SoundUnderAssumptions"/> and 181 of the 322 rules
        /// written as data are <see cref="Transformations.Soundness.Sound"/>; a step that fires one
        /// of those 181 now says so.
        /// </para>
        /// <para>
        /// The fallback is not a claim about the rule. Where <see cref="Rule"/> is null, or is an
        /// arm read off a <c>switch</c> and so declares no tier, what is known is the set's tier
        /// and that is what this gives.
        /// </para>
        /// </remarks>
        public Soundness Soundness => Rule?.Soundness ?? RuleSet.Soundness;

        /// <summary>
        /// This rewrite as a sentence: why it was allowed, then what it did.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>Dividing by a quotient multiplies by its reciprocal (a / (b / c) = a * c / b), so
        /// x / (y / z) becomes x * z / y.</c>
        /// </para>
        /// <para>
        /// Every word of it is read off the rule — the clause is
        /// <see cref="RewriteRule.Name"/> with its hyphens replaced, and the identity in brackets
        /// is <see cref="RewriteRule.Description"/>. Nothing is phrased a second time here, so a
        /// rule that is renamed or re-described says the new thing without this being touched.
        /// </para>
        /// <para>
        /// Where the rewrite is only addressable at set grain — <see cref="Rule"/> is null — the
        /// sentence attributes rather than explains: <c>x / (y / z) becomes x * z / y, by
        /// Common.</c> That is the whole of what is known, and dressing it as a reason would be
        /// inventing one.
        /// </para>
        /// </remarks>
        public string Explain() => Explanation.Sentence(RuleSet, Rule, Before, After);

        /// <inheritdoc/>
        public override string ToString()
            => Rule is null
                ? $"{RuleSet.Name}: {Before.Stringize()} -> {After.Stringize()}"
                : $"{RuleSet.Name}/{Rule.Name}: {Before.Stringize()} -> {After.Stringize()}";
    }
}
