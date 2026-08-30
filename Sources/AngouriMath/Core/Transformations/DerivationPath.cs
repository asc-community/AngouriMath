//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Core.Transformations
{
    /// <summary>
    /// One whole expression turning into another: the grain a derivation is read at.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Not the same thing as <see cref="RewriteStep"/>, and the difference is the whole point.
    /// A <see cref="RewriteStep"/> is one rule firing on one <i>subexpression</i>, somewhere in
    /// the middle of a pass; a <see cref="DerivationStep"/> is a pass, with the entire expression
    /// as it stood before it and as it stood after. The second is what can be chained — the
    /// <see cref="After"/> of one step is the <see cref="Before"/> of the next — and the first is
    /// what says which identity was used, which is why the rewrites are carried along in
    /// <see cref="Rewrites"/> rather than replaced by the pass that contained them.
    /// </para>
    /// </remarks>
    public readonly struct DerivationStep
    {
        internal DerivationStep(Entity before, Entity after, RewriteRuleSet? ruleSet, string name, IReadOnlyList<RewriteStep> rewrites)
            => (Before, After, RuleSet, Name, Rewrites) = (before, after, ruleSet, name, rewrites);

        /// <summary>The whole expression as it stood before this step.</summary>
        public Entity Before { get; }

        /// <summary>The whole expression as it stood after it. Never equal to <see cref="Before"/>.</summary>
        public Entity After { get; }

        /// <summary>
        /// Which registered rule set did it, where the step is one set applied once —
        /// <see langword="null"/> where the step is something else the simplifier does
        /// (inner simplification, expansion, factoring, polynomial rearrangement).
        /// </summary>
        /// <remarks>
        /// The same "null where it is not addressable" convention as
        /// <see cref="RewriteStep.Rule"/>: a step that is not a rule set has no rule set to
        /// name, and saying so is better than inventing one.
        /// </remarks>
        public RewriteRuleSet? RuleSet { get; }

        /// <summary>What did it, named. <see cref="RewriteRuleSet.Name"/> where there is one.</summary>
        public string Name { get; }

        /// <summary>
        /// The individual rewrites that fired inside this step, in the order they fired.
        /// </summary>
        /// <remarks>
        /// Not the same list as <see cref="RuleSet"/> being non-null. A step that is one rule set
        /// applied once carries what that set matched; a step that is a chain of them — the
        /// tidying pass every stage of the simplifier runs is six — carries everything that fired
        /// anywhere inside it, and names no single set. And empty is ordinary: a polynomial
        /// rearrangement or an expansion changes the expression without any rule matching.
        /// </remarks>
        public IReadOnlyList<RewriteStep> Rewrites { get; }

        /// <summary>What this step claims about its output, where it is a rule set. See <see cref="RewriteRuleSet.Relation"/>.</summary>
        public TransformationRelation? Relation => RuleSet?.Relation;

        /// <summary>
        /// How well justified that claim is — the weakest tier any rewrite inside it holds at,
        /// falling back to the set's where no rewrite was recorded. <see langword="null"/> where
        /// the step is not a rule set at all.
        /// </summary>
        /// <remarks>
        /// The weakest, because a step is only as justified as the least justified thing in it: a
        /// pass of nine unconditional rewrites and one conditional one is a conditional pass. Read
        /// off the rewrites rather than off the set, so that a pass of rules which all hold
        /// universally says so instead of inheriting its set's minimum over rules it did not fire.
        /// </remarks>
        public Soundness? Soundness
        {
            get
            {
                if (RuleSet is null) return null;
                var weakest = (Soundness?)null;
                foreach (var rewrite in Rewrites)
                    if (weakest is not { } known || rewrite.Soundness > known)
                        weakest = rewrite.Soundness;
                return weakest ?? RuleSet.Soundness;
            }
        }

        /// <summary>
        /// This pass as a sentence: what it did to the whole expression, and why.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A pass is not a rewrite, so this is not <see cref="RewriteStep.Explain"/> at another
        /// grain. Three shapes, and which one is used is a fact about the step rather than a
        /// choice:
        /// </para>
        /// <list type="bullet">
        /// <item>a pass in which <b>one</b> rewrite fired is that rewrite, so it is explained as
        /// one — anything else wraps a layer of narration around a single identity;</item>
        /// <item>a pass that only tidies says so and names no identity, because
        /// <see cref="RewriteRuleSet.IsNormalization"/> is the author saying this step is not where
        /// the mathematics happened;</item>
        /// <item>a pass of several rewrites names how many and what did them. The identities are in
        /// <see cref="Rewrites"/> for a reader who wants them, and repeating all of them here would
        /// make the chain unreadable at the grain it is read at.</item>
        /// </list>
        /// </remarks>
        public string Explain()
        {
            if (RuleSet is { IsNormalization: true })
                return $"Tidying: {Explanation.Transition(Before, After)}.";
            if (Rewrites.Count == 1)
                return Rewrites[0].Explain();
            if (Rewrites.Count == 0)
                return $"{Explanation.Transition(Before, After)}, by {Name}.";
            return $"{Explanation.Transition(Before, After)}, by {Rewrites.Count} rewrites of {Name}.";
        }

        /// <inheritdoc/>
        public override string ToString() => $"{Name}: {Before.Stringize()} -> {After.Stringize()}";
    }

    /// <summary>
    /// How an expression became an answer: an ordered chain of whole expressions, each one the
    /// result of the step before it, from the input to the value that was actually returned.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is what <see cref="RewriteRecording.Steps"/> and <see cref="RewriteRecording.Derivation"/>
    /// are not. Those are the rewrites that fired, across every candidate
    /// <see cref="Entity.Simplify(int)"/> generated including the ones it discarded, each on the
    /// subexpression it matched. Reading them in order does not walk from the input to the answer.
    /// This does: <c>Steps[i].After == Steps[i + 1].Before</c> holds for every <c>i</c>,
    /// <c>Steps[0].Before</c> is the <see cref="Input"/>, and the last step lands on the
    /// <see cref="Result"/>. Compared as expressions, not as printed forms.
    /// </para>
    /// <para>
    /// <b>The losing candidates are not here.</b> The simplifier explores far more expressions than
    /// it keeps — <see cref="ExpressionsExplored"/> says how many — and a route through a discarded
    /// candidate is not a route to the answer. They are excluded rather than labelled because a
    /// derivation is read forwards: a reader following the chain from the input needs every entry
    /// to be a step towards the answer, and a marked dead end is something to skip, which is the
    /// same as not being there while costing the reader the skip. What was explored but not taken
    /// is a fact about the search, and it is reported as a count for that reason.
    /// </para>
    /// <para>
    /// <b>Every step really happened.</b> Each one is an edge the engine actually traversed, with
    /// the expression it started from and the one it produced. Where several recorded routes reach
    /// the answer, the shortest is taken, and ties are settled by the order the engine recorded
    /// them in — so the same input gives the same path.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// using AngouriMath;
    /// using AngouriMath.Core.Transformations;
    ///
    /// foreach (var step in DerivationPath.OfSimplifying("x ^ (-1) / (y / z)")!.Steps)
    ///     Console.WriteLine(step);
    /// </code>
    /// </example>
    public sealed class DerivationPath
    {
        internal DerivationPath(Entity input, Entity result, IReadOnlyList<DerivationStep> steps, int expressionsExplored)
            => (Input, Result, Steps, ExpressionsExplored) = (input, result, steps, expressionsExplored);

        /// <summary>Where it started.</summary>
        public Entity Input { get; }

        /// <summary>Where it ended — the value the operation returned.</summary>
        public Entity Result { get; }

        /// <summary>
        /// The steps, in order. Empty where the input was already the answer, which is a path of
        /// length zero rather than a failure.
        /// </summary>
        public IReadOnlyList<DerivationStep> Steps { get; }

        /// <summary>
        /// How many distinct expressions the search produced on the way, of which
        /// <see cref="Steps"/> is the chain it kept.
        /// </summary>
        /// <remarks>
        /// Here so that the path does not read as though it were the whole of what happened. On
        /// <c>x^(-1)/(y/z)</c> the search produces eleven expressions, keeps four of them, and
        /// fires 270 rewrites doing it; a four-step derivation presented on its own says none of
        /// that.
        /// </remarks>
        public int ExpressionsExplored { get; }

        /// <summary>
        /// Runs <see cref="Entity.Simplify(int)"/> under a recording and returns how it got there,
        /// or <see langword="null"/> where the chain could not be reconstructed.
        /// </summary>
        /// <param name="expression">The expression to simplify.</param>
        /// <param name="level">As <see cref="Entity.Simplify(int)"/>.</param>
        /// <remarks>
        /// The whole of <a href="https://github.com/asc-community/AngouriMath/issues/28">#28</a> in
        /// one call, and it opens and closes the recording itself so that nothing is left on.
        /// </remarks>
        public static DerivationPath? OfSimplifying(Entity expression, int level = 2)
        {
            if (expression is null)
                throw new ArgumentNullException(nameof(expression));
            using var recording = RewriteRecording.Start();
            var result = expression.Simplify(level);
            return recording.PathFrom(expression, result);
        }

        /// <summary>
        /// The derivation in prose: what it did, then a numbered sentence per step, then what the
        /// whole of it is justified by.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a> tier 2 asks
        /// for "transformation metadata rich enough that v5.0 can render a step as a sentence".
        /// This is the check on that: the metadata is rich enough exactly when a sentence can be
        /// built from it, and every word of one here is read off a rule — the clause is the rule's
        /// name with its hyphens replaced, and the identity in brackets is its description.
        /// </para>
        /// <para>
        /// <b>The closing note is counted, not asserted.</b> How many steps hold universally, how
        /// many hold under assumptions, and how much of the search was discarded are all read off
        /// this path, so the paragraph cannot come to disagree with the steps above it — which is
        /// the failure a written-out summary has and a computed one does not.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// Console.WriteLine(DerivationPath.OfSimplifying("x ^ (-1) / (y / z)")!.Explain());
        /// </code>
        /// </example>
        public string Explain()
        {
            var text = new StringBuilder();
            if (Steps.Count == 0)
            {
                // Nothing was kept, which is not the same as nothing having been tried -- and the
                // difference is the whole of what this sentence has to say. The search below
                // reports it: on `(a + b) ^ 2` five expressions were reached and none was better,
                // where "nothing was done to it" would have described an engine that did not look.
                text.Append(Input.Stringize()).Append(" was already ").Append(Result.Stringize())
                    .Append('.');
            }
            else
            {
                text.Append(Input.Stringize()).Append(" becomes ").Append(Result.Stringize())
                    .Append(Steps.Count == 1 ? ", in one step." : $", in {Steps.Count} steps.")
                    .Append('\n');
                for (var i = 0; i < Steps.Count; i++)
                    text.Append('\n').Append(i + 1).Append(". ").Append(Steps[i].Explain());
            }

            var tiers = new List<Soundness>();
            foreach (var step in Steps)
                if (step.Soundness is { } tier)
                    tiers.Add(tier);
            var closing = new StringBuilder();
            if (Explanation.TierNote(tiers, Steps.Count) is { } note)
                closing.Append(note);
            // The search, so that the chain does not read as the whole of what happened. Only
            // where something was discarded: on a path that explored nothing else there is no
            // discrepancy to explain, and saying so anyway is a sentence that never varies.
            if (ExpressionsExplored > Steps.Count)
            {
                if (closing.Length > 0) closing.Append(' ');
                closing.Append("The search reached ").Append(ExpressionsExplored)
                    .Append(Steps.Count == 0
                        ? " expressions and kept none of them."
                        : $" expressions and kept these {Steps.Count}.");
            }
            if (closing.Length > 0)
                text.Append(Steps.Count == 0 ? " " : "\n\n").Append(closing);
            return text.ToString();
        }

        /// <summary>
        /// The path written out: the input, then one line per step giving what the expression
        /// became and what made it so.
        /// </summary>
        /// <remarks>
        /// The names are lined up in a column, which is what makes a derivation scannable — the
        /// reader is looking down the list of identities used, not reading the expressions.
        /// <para/>
        /// <see cref="Explain"/> is the same path for the other reader: prose, naming the identity
        /// rather than the rule set.
        /// </remarks>
        public override string ToString()
        {
            var stages = new string[Steps.Count];
            var column = 0;
            for (var i = 0; i < Steps.Count; i++)
            {
                stages[i] = "  = " + Steps[i].After.Stringize();
                if (stages[i].Length > column)
                    column = stages[i].Length;
            }
            var text = new StringBuilder(Input.Stringize());
            for (var i = 0; i < stages.Length; i++)
                text.Append('\n').Append(stages[i]).Append(' ', column - stages[i].Length)
                    .Append("   // ").Append(Steps[i].Name);
            return text.ToString();
        }
    }
}
