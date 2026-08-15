//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Generic;

namespace AngouriMath.Core.Transformations
{
    /// <summary>
    /// Which way a rewrite moves: does it make the expression bigger, smaller, or neither.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Counted from what the rule is written as — operators plus operands on the pattern side
    /// against operators plus operands on the replacement side — and therefore a statement about
    /// the rule, not about any particular expression it fires on.
    /// </para>
    /// <para>
    /// It exists because a rewrite <i>graph</i> needs it and a rewrite <i>pipeline</i> does not.
    /// <see cref="Entity.Simplify(int)"/> applies a set, keeps a candidate and moves on, so an
    /// expanding rule and a collecting one never meet: the order they run in decides which wins.
    /// Equality saturation deletes that order and keeps both results, so it has to be told which
    /// pairs undo each other or it will grow without bound —
    /// <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a> tier 2, measured
    /// in the <c>egraph</c> harness at up to 7,143 times the e-nodes when it is not told.
    /// </para>
    /// </remarks>
    public enum RewriteRuleGrowth
    {
        /// <summary>The replacement is written with fewer operators and operands than the pattern.</summary>
        Collects,

        /// <summary>The two are written with the same number, so the rule moves things about.</summary>
        Rearranges,

        /// <summary>The replacement is written with more, so the rule opens the expression out.</summary>
        Expands
    }

    /// <summary>
    /// One rewrite, addressable on its own: what it matches, what it puts there instead, where it
    /// is written and which way it moves.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="RewriteRuleSet"/> is the unit the library applies; this is the unit inside it.
    /// The distinction is what
    /// <a href="https://github.com/asc-community/AngouriMath/issues/28">#28</a> asks for — a
    /// derivation that says <i>which rewrite</i> fired rather than which group of them — and what
    /// <a href="https://github.com/asc-community/AngouriMath/issues/825">#825</a> is about.
    /// </para>
    /// <para>
    /// These are <b>generated from the <c>switch</c> that defines them</b>, arm by arm, rather
    /// than written out a second time. That is deliberate and it is the whole design: the
    /// <c>switch</c> stays the thing a human edits and the thing the simplifier calls, so nothing
    /// on the hot path changes and the two forms cannot drift apart. Transcribing forty arms into
    /// forty objects by hand would be forty chances to alter a pattern silently, and expressing
    /// them through the runtime matcher in
    /// <c>AngouriMath.Core.Transformations.Matching</c> was measured at about five percent of
    /// <see cref="Entity.Simplify(int)"/> per rule set exchanged.
    /// </para>
    /// <para>
    /// <b>What is not here.</b> A per-rule <see cref="Soundness"/>. A rule's tier is a claim
    /// somebody has to argue for, and there is no honest way to derive one from syntax — so
    /// <see cref="RewriteRuleSet.Soundness"/> remains the declared tier and this type does not
    /// invent a finer one it cannot justify. What being addressable buys is that the finer tier
    /// now has somewhere to live once the argument is made, which it did not before.
    /// </para>
    /// </remarks>
    public sealed class RewriteRule
    {
        internal RewriteRule(
            string source,
            int index,
            string name,
            string? description,
            IReadOnlyList<Type> nodeTypes,
            string patternSource,
            string? guardSource,
            string replacementSource,
            RewriteRuleGrowth growth,
            int sourceLine,
            Func<Entity, Entity?> apply)
        {
            Source = source;
            Index = index;
            Name = name;
            Description = description;
            NodeTypes = nodeTypes;
            PatternSource = patternSource;
            GuardSource = guardSource;
            ReplacementSource = replacementSource;
            Growth = growth;
            SourceLine = sourceLine;
            this.apply = apply;
        }

        private readonly Func<Entity, Entity?> apply;

        /// <summary>The method whose <c>switch</c> this arm belongs to.</summary>
        public string Source { get; }

        /// <summary>Where it sits among that method's arms, which is the order it is tried in.</summary>
        /// <remarks>
        /// First match wins, so a rule's index is part of what it does: two rules that can both
        /// fire on one node are resolved by this and nothing else.
        /// </remarks>
        public int Index { get; }

        /// <summary>
        /// What to call this rule in a report, a test or a bug — the pattern it matches, written
        /// as the source writes it.
        /// </summary>
        /// <remarks>
        /// The pattern rather than the position, because a position moves whenever an arm is
        /// inserted above it and the point of a name is to survive that. Where a set really does
        /// write one pattern twice, the later ones are suffixed <c>#2</c>, <c>#3</c> — and a set
        /// that does is worth looking at, since the second is unreachable.
        /// </remarks>
        public string Name { get; }

        /// <summary>The comment written above the rule, where there is one: the identity in the notation a mathematician would use.</summary>
        public string? Description { get; }

        /// <summary>
        /// The node types the rule can fire on — usually one, occasionally two, and empty where
        /// the pattern's shape does not say.
        /// </summary>
        /// <remarks>
        /// A <b>necessary</b> condition, not a sufficient one: a node of one of these types may
        /// still fail the rest of the pattern. That is the direction that makes it useful, since
        /// what it licenses is skipping the rule on every other type — which is the dispatch a
        /// large <c>switch</c> over distinct node types gets from the compiler for free and a
        /// list of rules has to be told.
        /// </remarks>
        public IReadOnlyList<Type> NodeTypes { get; }

        /// <summary>The pattern the arm matches, as the C# source writes it.</summary>
        /// <remarks>
        /// <para>
        /// <b>Source text, and named so.</b> This is what a reader would see in the
        /// <c>switch</c>, not a representation anything can match against — to ask whether this
        /// rule fires on a node, call <see cref="TryApply(Entity)"/>, which runs the arm itself.
        /// </para>
        /// <para>
        /// The name carries <c>Source</c> because
        /// <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a> tier 1 is
        /// pattern matching as data, and when a pattern becomes a value it should be able to be
        /// called <c>Pattern</c> without first breaking somebody. Deciding that after the
        /// property shipped would have cost a major version; deciding it here cost nothing.
        /// </para>
        /// </remarks>
        public string PatternSource { get; }

        /// <summary>
        /// The side condition as the C# source writes it, or <see langword="null"/> where the arm
        /// has no <c>when</c> clause. Source text — see <see cref="PatternSource"/>.
        /// </summary>
        public string? GuardSource { get; }

        /// <summary>
        /// What the arm builds, as the C# source writes it. Source text — see
        /// <see cref="PatternSource"/>.
        /// </summary>
        public string ReplacementSource { get; }

        /// <summary>Which way the rewrite moves. See <see cref="RewriteRuleGrowth"/>.</summary>
        public RewriteRuleGrowth Growth { get; }

        /// <summary>The line of the source file the arm is written on.</summary>
        public int SourceLine { get; }

        /// <summary>
        /// This one rule at this one node, ignoring its children and every other rule — or
        /// <see langword="null"/> where it does not apply.
        /// </summary>
        /// <remarks>
        /// Null means "this rule does not fire here", which is a different claim from the rule
        /// set's <see cref="RewriteRuleSet.ApplyOnce(Entity)"/> handing back the expression it was
        /// given. The set has to return something; a rule may decline.
        /// </remarks>
        public Entity? TryApply(Entity node)
            => node is null ? throw new ArgumentNullException(nameof(node)) : apply(node);

        /// <inheritdoc/>
        public override string ToString()
            => GuardSource is null
                ? $"{PatternSource} => {ReplacementSource}"
                : $"{PatternSource} when {GuardSource} => {ReplacementSource}";
    }
}
