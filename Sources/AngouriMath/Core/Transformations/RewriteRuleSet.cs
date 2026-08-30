//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;

namespace AngouriMath.Core.Transformations
{
    /// <summary>
    /// A named, attributable group of rewrites — the unit this library has always written
    /// them in — carrying what it is called, what it claims and how well justified the
    /// claim is, so that the set can be enumerated, tested and referred to by name instead
    /// of only being called.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The set is the unit the library <i>applies</i>; <see cref="Rules"/> is the unit inside
    /// it. Those rules are generated from the <c>switch</c> that defines the set rather than
    /// written out again, so the <c>switch</c> stays the thing a human edits and the thing
    /// this calls, and the two cannot drift apart. See
    /// <a href="https://github.com/asc-community/AngouriMath/issues/825">#825</a> and
    /// <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a> item 50.
    /// </para>
    /// </remarks>
    public sealed class RewriteRuleSet
    {
        private readonly Func<Entity, Entity> rules;

        internal RewriteRuleSet(string name, string description, TransformationRelation relation, Soundness soundness, Func<Entity, Entity> rules, IReadOnlyList<RewriteRule>? addressable = null, bool isNormalization = false)
            => (Name, Description, Relation, Soundness, this.rules, Rules, IsNormalization)
                = (name, description, relation, soundness, rules, addressable ?? Array.Empty<RewriteRule>(), isNormalization);

        /// <summary>
        /// A set whose rewrites are written as data: it both <i>runs</i>
        /// <paramref name="source"/> and <i>describes</i> it.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>One argument rather than two, because two could disagree.</b> The other constructor
        /// takes what to run and what to report separately, and for most of the registry's life
        /// those were a matcher on one side and a <c>switch</c> read by
        /// <c>RuleRegistryGenerator</c> on the other — so a set could describe arms it no longer
        /// executed, and 26 of them did. Passing the set itself makes that unrepresentable.
        /// </para>
        /// <para>
        /// It also takes the factory call off the caller. A set built by a parameterised factory —
        /// the common-denominator family, one definition read at three sort levels — would
        /// otherwise be constructed twice, once for the delegate and once for the rules, and the
        /// description would be of a different instance from the one that runs.
        /// </para>
        /// </remarks>
        internal RewriteRuleSet(string name, string description, TransformationRelation relation, Soundness soundness, Matching.MatchedRuleSet source, bool isNormalization = false)
            : this(name, description, relation, soundness,
                   (source ?? throw new ArgumentNullException(nameof(source))).ApplyHere,
                   source.AsAddressable(), isNormalization)
        {
        }

        /// <summary>A stable identity for this set.</summary>
        public string Name { get; }

        /// <summary>What the set is for, in a sentence.</summary>
        public string Description { get; }

        /// <summary>What the rewrites in this set claim about the expressions they produce.</summary>
        public TransformationRelation Relation { get; }

        /// <summary>How well justified that claim is. See <see cref="Soundness"/> on what a tier here is and is not.</summary>
        public Soundness Soundness { get; }

        /// <summary>
        /// Whether this set only puts an expression into a canonical shape, rather than moving it
        /// towards an answer.
        /// </summary>
        /// <remarks>
        /// Declared by the set rather than inferred, because it is a statement about intent that
        /// no amount of looking at a rewrite settles: reordering <c>y + x</c> to <c>x + y</c> and
        /// collapsing <c>x + x</c> to <c>2 * x</c> are both equivalences that change the tree, and
        /// only the author knows which one was meant as tidying.
        /// <para/>
        /// What it is for: a reader following a derivation wants the rewrites that got somewhere,
        /// and normalisation is the engine straightening the expression between them. On
        /// <c>x^(-1)/(y/z)</c> it is 251 of the 270 recorded rewrites. See
        /// <see cref="RewriteRecording.Derivation"/> and
        /// <a href="https://github.com/asc-community/AngouriMath/issues/28">#28</a>.
        /// </remarks>
        public bool IsNormalization { get; }

        /// <summary>
        /// The individual rewrites this set is made of, in the order they are tried.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Empty is not the same as "no rewrites".</b> A set whose rewrites are written as a
        /// <c>switch</c> over the expression has every arm listed here, whether the
        /// <c>switch</c> is the rule method itself or sits inside a factory parameterised by
        /// something else — the sorts are the latter, one <c>switch</c> read at three levels. A
        /// set built without one — a polynomial division, a method with branches and locals, a
        /// single <c>is</c> pattern that is one rule and has nothing to split — has none, because
        /// there are no arms to generate from. <see cref="Name"/> and
        /// <see cref="ApplyOnce(Entity)"/> behave identically either way, so this is a statement
        /// about how finely the set can be reported on and not about what it does.
        /// </para>
        /// <para>
        /// First match wins, exactly as in the <c>switch</c>, so the order is part of the
        /// meaning: where two rules can fire on one node, the earlier one does.
        /// </para>
        /// </remarks>
        public IReadOnlyList<RewriteRule> Rules { get; }

        /// <summary>
        /// The rule that fires at this node, or <see langword="null"/> where none does.
        /// </summary>
        /// <remarks>
        /// <para>
        /// At this node only, leaving its children alone — which is what an arm of the
        /// <c>switch</c> sees. Always null for a set with no <see cref="Rules"/>.
        /// </para>
        /// <para>
        /// <b>Firing means changing the node.</b> An arm can match and then decline: the factorial
        /// arms match any quotient of factorials and hand back what they were given where the two
        /// offsets are a hundred apart, since writing that product out is not a simplification.
        /// Naming such an arm here would report a rewrite that did not happen, so a rule that
        /// returns the node it was given is not the rule that fired.
        /// </para>
        /// </remarks>
        public RewriteRule? RuleFiringAt(Entity node)
        {
            if (node is null)
                throw new ArgumentNullException(nameof(node));
            // The array, and by index: this is walked once per recorded rewrite and the sets are
            // long -- Common alone has 100 arms, and InequalityEquality 65.
            for (var i = 0; i < Rules.Count; i++)
                if (Rules[i].TryApply(node) is { } rewritten && rewritten != node)
                    return Rules[i];
            return null;
        }

        /// <summary>
        /// Applies the set once, bottom-up over every node, exactly as
        /// <see cref="Entity.Replace(Func{Entity, Entity})"/> does. One pass: a rewrite
        /// that opens up an opportunity for another rewrite in the same set will not see it
        /// taken until the next pass.
        /// </summary>
        /// <param name="expression">The expression to rewrite.</param>
        public Entity ApplyOnce(Entity expression)
        {
            if (expression is null)
                throw new ArgumentNullException(nameof(expression));

            // One thread-static read, per application rather than per node, and nothing
            // allocated: the ordinary path must not pay for a recording nobody opened.
            var recording = RewriteRecording.Current;
            return recording is null
                ? expression.Replace(rules)
                : ApplyOnceRecording(expression, recording);
        }

        /// <remarks>
        /// A method of its own, and that is the whole reason for it. The closure below
        /// captures the rule set and the recording, and the compiler allocates the object
        /// holding them where they come into scope -- so writing this inline in
        /// <see cref="ApplyOnce(Entity)"/> put one allocation on every rewrite in the
        /// library whether or not anybody was recording. Measured: it cost `Simplify` a
        /// fifth of its allocation on the benchmark expressions.
        /// </remarks>
        private Entity ApplyOnceRecording(Entity expression, RewriteRecording recording)
        {
            var mark = recording.Mark();
            var rewritten = expression.Replace(node =>
            {
                var inner = rules(node);
                if (inner != node)
                    // Which rule did it, asked only of a node that actually changed and only
                    // while somebody is recording. The arms are tried in the same order the
                    // switch tries them, so the first that applies is the one that fired.
                    recording.Add(this, RuleFiringAt(node), node, inner);
                return inner;
            });
            // And the pass as a whole, which is the grain a derivation is read at: the steps
            // above are subexpressions, and only this has the expression that contained them
            // before and after. See RewriteRecording.PathFrom.
            if (rewritten != expression)
                recording.Note(expression, rewritten, this, Name, mark);
            return rewritten;
        }

        /// <summary>
        /// This set as a <see cref="Transformation"/>, so that it composes with the rest of
        /// the catalogue.
        /// </summary>
        /// <remarks>
        /// Built on demand, and it has to be. <see cref="RewritingTransformation"/> derives
        /// from <see cref="Transformation"/>, so constructing one runs that type's static
        /// initialiser -- which reads <see cref="RewriteRules"/>. Doing it in this
        /// constructor instead would make the two types depend on each other's
        /// initialisation and hand whichever one lost the race a null rule set. Two threads
        /// arriving together may each build one; they are equivalent and immutable, so
        /// whichever reference lands is the one everyone then uses.
        /// </remarks>
        public Transformation AsTransformation() => asTransformation ??= new RewritingTransformation(this);
        private Transformation? asTransformation;

        /// <inheritdoc/>
        public override string ToString() => Name;

        private sealed class RewritingTransformation : Transformation
        {
            private readonly RewriteRuleSet ruleSet;

            internal RewritingTransformation(RewriteRuleSet ruleSet) => this.ruleSet = ruleSet;

            public override string Name => $"rewrite[{ruleSet.Name}]";

            public override TransformationRelation Relation => ruleSet.Relation;

            public override Soundness Soundness => ruleSet.Soundness;

            // A rewrite pass always has an answer: where nothing matched, the answer is the
            // expression it was given. That is a fixed point, not a failure, and
            // TransformationResult.Changed is what tells the two apart.
            protected override Entity? ApplyCore(Entity input) => ruleSet.ApplyOnce(input);
        }
    }
}
