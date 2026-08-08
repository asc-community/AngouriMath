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
    /// The set, rather than the single <c>pattern -&gt; replacement</c> line, is the unit
    /// here because that is what has been built so far — <b>not</b> because the finer grain
    /// is too expensive. It was asserted here that it would be, on the grounds that each
    /// rule would cost a delegate call per node; measuring it found the opposite, and the
    /// measurement is in
    /// <a href="https://github.com/asc-community/AngouriMath/issues/825">#825</a>: rules
    /// bucketed by the node type they match run as fast as the hand-written <c>switch</c>
    /// at realistic set sizes and faster at small ones, because a large <c>switch</c> over
    /// distinct node types is compiled into that same dispatch anyway.
    /// </para>
    /// <para>
    /// What actually stands in the way is transcription: splitting forty <c>switch</c> arms
    /// by hand is forty chances to change a pattern silently. That points at a source
    /// generator over the existing bodies rather than at leaving the rewrites unnamed. See
    /// <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a> item 50,
    /// and note that nothing here forecloses it: a set whose rewrites become individually
    /// addressable keeps the same name and the same entry in the registry.
    /// </para>
    /// </remarks>
    public sealed class RewriteRuleSet
    {
        private readonly Func<Entity, Entity> rules;

        internal RewriteRuleSet(string name, string description, TransformationRelation relation, Soundness soundness, Func<Entity, Entity> rules)
            => (Name, Description, Relation, Soundness, this.rules) = (name, description, relation, soundness, rules);

        /// <summary>A stable identity for this set.</summary>
        public string Name { get; }

        /// <summary>What the set is for, in a sentence.</summary>
        public string Description { get; }

        /// <summary>What the rewrites in this set claim about the expressions they produce.</summary>
        public TransformationRelation Relation { get; }

        /// <summary>How well justified that claim is. See <see cref="Soundness"/> on what a tier here is and is not.</summary>
        public Soundness Soundness { get; }

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
            => expression.Replace(node =>
            {
                var rewritten = rules(node);
                if (rewritten != node)
                    recording.Add(this, node, rewritten);
                return rewritten;
            });

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
