//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace AngouriMath.Core.Transformations
{
    /// <summary>
    /// Collects the rewrites that fire while it is open, so that an answer can be asked how
    /// it was reached rather than only what it is.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Off unless asked for, and off is free: with no recording open, applying a rule set
    /// costs one ambient read more than it did before — per rule set, not per node — and
    /// allocates nothing. That is the condition
    /// <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a> puts on
    /// every layer above the tree, and it is why this is a scope rather than a setting that
    /// something might leave on.
    /// </para>
    /// <para>
    /// Per flow, like <see cref="MathS.Settings"/>: the recording belongs to the call rather
    /// than to the thread running it. It survives an <see langword="await"/>, and work
    /// started under it — including on another thread — reports to it. A recording opened
    /// inside a task is invisible to that task's siblings and to whatever started it.
    /// </para>
    /// <para>
    /// <b>Order is not guaranteed once work is parallel.</b> Steps from one flow keep the
    /// order they fired in, but two flows recording into the same open recording interleave
    /// however they happen to run. The single-threaded case — which is what
    /// <see cref="Entity.Simplify(int)"/> is — is unaffected.
    /// </para>
    /// <para>
    /// <b>Three views, and they answer different questions.</b> <see cref="Steps"/> is every
    /// rewrite that fired, on the subexpression it matched, across every candidate
    /// <see cref="Entity.Simplify(int)"/> generated including the ones that lost.
    /// <see cref="Derivation"/> is the same list with the normalisation and the repeats taken
    /// out — 270 rewrites down to 5 on <c>x^(-1)/(y/z)</c> — and it is still a *set* of
    /// rewrites, so reading it in order does not walk from the input to the answer.
    /// <see cref="PathFrom(Entity, Entity)"/> is the one that does: whole expressions, in
    /// order, from the input to the value that was returned, with the losing candidates left
    /// out. Ask the first what fired, the second which identities were used, the third how it
    /// got there.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// using AngouriMath;
    /// using AngouriMath.Core.Transformations;
    ///
    /// using var recording = RewriteRecording.Start();
    /// var simplified = ((Entity)"a / (b / c)").Simplify();
    /// foreach (var step in recording.Steps)
    ///     Console.WriteLine(step);
    /// </code>
    /// </example>
    public sealed class RewriteRecording : IDisposable
    {
        /// <summary>
        /// Which recording the current call reports to. An <see cref="AsyncLocal{T}"/> rather
        /// than <c>[ThreadStatic]</c>, so the scope follows the call: held per thread, a
        /// recording was lost at the first <see langword="await"/>, and a pool thread carried
        /// a stale one into whoever borrowed it next.
        /// </summary>
        [ConcurrentField]
        private static readonly AsyncLocal<RewriteRecording?> current = new();

        private readonly RewriteRecording? enclosing;

        /// <summary>
        /// Concurrent because the pointer above flows into child tasks, so two of them can
        /// report to one recording at once. A <see cref="List{T}"/> here would be a torn
        /// write rather than a merged list.
        /// </summary>
        private readonly ConcurrentQueue<RewriteStep> steps = new();

        /// <summary>
        /// One whole expression turning into another, which is what a path is made of. Kept
        /// beside <see cref="steps"/> rather than derived from it, because a rewrite pass
        /// records the subexpressions it changed and never the expression that contained
        /// them — so the two grains are different measurements and neither reconstructs the
        /// other.
        /// </summary>
        private readonly ConcurrentQueue<Edge> edges = new();

        /// <summary>
        /// How many rewrites have been recorded, so that an edge can say which of them fired
        /// inside it by index. <see cref="ConcurrentQueue{T}.Count"/> walks the segments, and
        /// this is read twice per edge.
        /// </summary>
        private int recorded;

        private volatile bool closed;

        private RewriteRecording(RewriteRecording? enclosing) => this.enclosing = enclosing;

        /// <summary>
        /// Opens a recording for this call. Dispose it to close it — the value is meant to
        /// be held in a <see langword="using"/>, as <see cref="MathS.Settings"/> values are.
        /// </summary>
        /// <remarks>
        /// Recordings nest: opening one inside another hides the outer one until the inner
        /// is disposed, so a caller who records a subcomputation does not silently add its
        /// steps to somebody else's list.
        /// </remarks>
        public static RewriteRecording Start()
        {
            var recording = new RewriteRecording(current.Value);
            current.Value = recording;
            return recording;
        }

        /// <summary>
        /// The rewrites that fired while this recording was open, in the order they fired.
        /// </summary>
        /// <remarks>
        /// A snapshot taken when you ask, not a live view, since the underlying store has to
        /// tolerate concurrent writers. Reading it after disposing — which is the usual way —
        /// gives the complete list either way.
        /// </remarks>
        public IReadOnlyList<RewriteStep> Steps => steps.ToArray();

        /// <summary>
        /// <see cref="Steps"/> with the tidying taken out and the repeats collapsed — the rewrites
        /// that are worth reading. <a href="https://github.com/asc-community/AngouriMath/issues/28">#28</a>
        /// </summary>
        /// <remarks>
        /// <para>
        /// Two things are dropped, and neither is a judgement about importance. Sets that declare
        /// <see cref="RewriteRuleSet.IsNormalization"/> are the engine straightening an expression
        /// between real rewrites. And a rewrite that has already appeared is not shown again: the
        /// simplifier explores several candidate forms and rewrites the same subexpression the same
        /// way in each, so the raw list repeats itself many times over.
        /// </para>
        /// <para>
        /// On <c>x^(-1)/(y/z)</c> that is 270 recorded rewrites down to 5.
        /// </para>
        /// <para>
        /// <b>This is a set of rewrites, not a path.</b> Each entry is a real rewrite that really
        /// fired, on the subexpression it names — but <see cref="Entity.Simplify(int)"/> searches candidate
        /// forms and keeps the best, so these come from several branches and some belong to
        /// candidates that were discarded. Reading them in order does not walk from the input to
        /// the answer, and a step's <see cref="RewriteStep.Before"/> is a subexpression rather than
        /// the whole expression at that moment. <see cref="PathFrom(Entity, Entity)"/> is the view
        /// that does walk it, at the grain of whole expressions.
        /// </para>
        /// </remarks>
        public IReadOnlyList<RewriteStep> Derivation
        {
            get
            {
                var seen = new HashSet<(string, string, string)>();
                var derivation = new List<RewriteStep>();
                foreach (var step in steps)
                {
                    if (step.RuleSet.IsNormalization)
                        continue;
                    // Keyed on what the reader sees — which rewrite, from what, to what — so that
                    // the same rewrite found down two candidate branches is shown once.
                    if (seen.Add((step.Rule?.Name ?? step.RuleSet.Name,
                                  step.Before.Stringize(), step.After.Stringize())))
                        derivation.Add(step);
                }
                return derivation;
            }
        }

        /// <summary>Closes the recording. <see cref="Steps"/> stays readable afterwards.</summary>
        public void Dispose()
        {
            if (closed)
                return;
            closed = true;
            // Only this flow's chain is ours to put back. Disposing from a flow that did not
            // open it would otherwise clear whatever that flow was recording into. Add
            // ignores a closed recording, so the worst a stray reference can do is nothing.
            if (ReferenceEquals(current.Value, this))
                current.Value = enclosing;
        }

        /// <summary>
        /// The recording to report to, or <see langword="null"/> where nobody is listening —
        /// which is the case this has to stay free for.
        /// </summary>
        internal static RewriteRecording? Current => current.Value;

        internal void Add(RewriteRuleSet ruleSet, RewriteRule? rule, Entity before, Entity after)
        {
            if (closed)
                return;
            steps.Enqueue(new RewriteStep(ruleSet, rule, before, after));
            Interlocked.Increment(ref recorded);
        }

        /// <summary>Where the rewrite list stands now, so that an edge can bracket its own.</summary>
        internal int Mark() => Volatile.Read(ref recorded);

        /// <summary>
        /// One whole expression having become another. <paramref name="from"/> is the
        /// <see cref="Mark()"/> taken before the work started.
        /// </summary>
        internal void Note(Entity before, Entity after, RewriteRuleSet? ruleSet, string name, int from)
        {
            if (closed)
                return;
            edges.Enqueue(new Edge(before, after, ruleSet, name, from, Mark()));
        }

        /// <summary>
        /// How <paramref name="input"/> became <paramref name="result"/>: the steps, in order,
        /// each one a whole expression. <see langword="null"/> where this recording holds no
        /// chain of steps joining the two.
        /// </summary>
        /// <param name="input">The expression the work started from.</param>
        /// <param name="result">The value it returned.</param>
        /// <remarks>
        /// <para>
        /// Reconstructed rather than logged, and it has to be: the simplifier does not walk one
        /// expression to an answer, it grows a family of candidates and keeps the cheapest, so
        /// "the route" only exists once the winner is known. Every edge here is one the engine
        /// really traversed; what this does is pick out the ones joining these two ends.
        /// </para>
        /// <para>
        /// The shortest such chain is taken, breadth-first over the edges in the order they were
        /// recorded — so the answer is the same every time for the same work, and a candidate the
        /// search abandoned cannot appear, since nothing leads from it to the result.
        /// </para>
        /// <para>
        /// <see langword="null"/> means "this recording cannot account for that", not "there was
        /// no route". It is what you get from a result this recording never saw produced — a
        /// different operation, or work done before the recording opened.
        /// </para>
        /// </remarks>
        public DerivationPath? PathFrom(Entity input, Entity result)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (result is null)
                throw new ArgumentNullException(nameof(result));

            var all = edges.ToArray();
            var produced = new HashSet<Entity>();
            var outgoing = new Dictionary<Entity, List<int>>();
            for (var i = 0; i < all.Length; i++)
            {
                produced.Add(all[i].After);
                if (!outgoing.TryGetValue(all[i].Before, out var fromHere))
                    outgoing[all[i].Before] = fromHere = new List<int>();
                fromHere.Add(i);
            }

            // Already there. A path of length zero, and not a failure: `2 + 2` simplifies to
            // `4` before the candidate search starts, and asking how is a fair question with
            // "it did not have to do anything" as the answer.
            if (input == result)
                // Nothing was abandoned to get nowhere: the input was already the answer.
                return new DerivationPath(
                    input, result, Array.Empty<DerivationStep>(),
                    Array.Empty<DerivationStep>(), produced.Count);

            var reachedBy = new Dictionary<Entity, int>();
            var seen = new HashSet<Entity> { input };
            var frontier = new Queue<Entity>();
            frontier.Enqueue(input);
            var arrived = false;
            while (frontier.Count > 0 && !arrived)
                if (outgoing.TryGetValue(frontier.Dequeue(), out var fromHere))
                    foreach (var edge in fromHere)
                    {
                        var next = all[edge].After;
                        if (!seen.Add(next))
                            continue;
                        reachedBy[next] = edge;
                        if (next == result)
                        {
                            arrived = true;
                            break;
                        }
                        frontier.Enqueue(next);
                    }
            if (!arrived)
                return null;

            var chain = new List<int>();
            for (var at = result; reachedBy.TryGetValue(at, out var edge); at = all[edge].Before)
                chain.Add(edge);
            chain.Reverse();

            var recordedSteps = steps.ToArray();
            var path = new List<DerivationStep>(chain.Count);
            foreach (var edge in chain)
                path.Add(Step(all, edge, recordedSteps));

            // Everything the search tried and came back from. The chain above is the branch that
            // survived, and handing it over on its own reads as though the library had walked
            // straight to the answer -- which #273's second half is precisely about.
            //
            // Deduplicated, and that is not tidying. The simplifier runs the same passes over the
            // same expressions at every level of its candidate search, so the raw edges are
            // mostly one rewrite recorded over and over: `x^(-1)/(y/z)` produces 425 of them
            // across 8 distinct steps, and one expression's list included the very edge the kept
            // chain had taken. A list like that is not a record of where the search went, it is a
            // record of how often it was asked.
            // https://github.com/asc-community/AngouriMath/issues/273
            var kept = new HashSet<int>(chain);
            var seenStep = new HashSet<(Entity, Entity, string)>();
            foreach (var edge in chain)
                seenStep.Add((all[edge].Before, all[edge].After, all[edge].Name));
            var abandoned = new List<DerivationStep>();
            for (var edge = 0; edge < all.Length; edge++)
            {
                if (kept.Contains(edge))
                    continue;
                if (!seenStep.Add((all[edge].Before, all[edge].After, all[edge].Name)))
                    continue;
                abandoned.Add(Step(all, edge, recordedSteps));
            }

            return new DerivationPath(input, result, path, abandoned, produced.Count);
        }

        /// <summary>One recorded edge as a <see cref="DerivationStep"/>, with the rewrites that
        /// fired inside it attached.</summary>
        private static DerivationStep Step(Edge[] all, int edge, RewriteStep[] recordedSteps)
        {
            var (before, after, ruleSet, name, from, to) = all[edge];
            // Clamped because a recording that is still being written to can hand back
            // fewer rewrites than an edge counted, and a torn read is not worth throwing over.
            from = Math.Min(from, recordedSteps.Length);
            to = Math.Min(to, recordedSteps.Length);
            var rewrites = to > from ? new RewriteStep[to - from] : Array.Empty<RewriteStep>();
            Array.Copy(recordedSteps, from, rewrites, 0, rewrites.Length);
            return new DerivationStep(before, after, ruleSet, name, rewrites);
        }

        /// <summary>One recorded whole-expression step, before the rewrites inside it are attached.</summary>
        private readonly struct Edge
        {
            internal Edge(Entity before, Entity after, RewriteRuleSet? ruleSet, string name, int from, int to)
                => (this.before, this.after, this.ruleSet, this.name, this.from, this.to)
                    = (before, after, ruleSet, name, from, to);

            private readonly Entity before, after;
            private readonly RewriteRuleSet? ruleSet;
            private readonly string name;
            private readonly int from, to;

            internal Entity After => after;

            internal Entity Before => before;

            /// <summary>What did this step, named. Read when deduplicating the abandoned
            /// branches, where two edges are the same step if they are between the same two
            /// expressions and were done by the same thing.</summary>
            internal string Name => name;

            internal void Deconstruct(out Entity before, out Entity after, out RewriteRuleSet? ruleSet, out string name, out int from, out int to)
                => (before, after, ruleSet, name, from, to) = (this.before, this.after, this.ruleSet, this.name, this.from, this.to);
        }
    }
}
