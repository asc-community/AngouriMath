//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AngouriMath.Core;

namespace AngouriMath
{
    /// <summary>
    /// One step in the decision of a quantified statement: the statement that was decided, the
    /// rule that decided it, the verdict, and how deep it sits under the statement asked --
    /// the base case of an induction is one below the induction, the identity that settles the
    /// step one below that. The rule is named as the reference names it and as a checker
    /// would: <see cref="Lemma"/> is the Lean 4 tactic or lemma the step corresponds to, which
    /// is what an export to a proof checker needs and the design constraint of
    /// <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a>'s proof engine.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1409">#1409</a>
    /// </summary>
    public sealed record ProofStep(Entity Statement, string Rule, string Lemma, Entity Verdict, int Depth)
    {
        /// <inheritdoc/>
        public override string ToString()
            => $"{new string(' ', 2 * Depth)}{Statement} is {Verdict}: {Rule} [{Lemma}]";
    }

    /// <summary>
    /// A scope that collects the steps by which the quantified statements decided inside it were
    /// decided, the way <see cref="Core.Transformations.RewriteRecording"/> collects rewrites: the proof of
    /// <c>forall n in ZZ+ : sum(k, k, 1, n) = n (n + 1)/2</c> is the closed form read through
    /// its case, the proof of <c>forall n in ZZ+ : sum(1/(k (k + 1)), k, 1, n) = n/(n + 1)</c>
    /// is a base case and a step, each a step here. Nothing is collected outside a scope, and
    /// the ordinary path does not pay for one nobody opened. A decision already made is not
    /// made again: an entity whose <see cref="Entity.Evaled"/> is cached, or a statement parsed
    /// through the parser's cache and evaluated before, records nothing, so a proof is asked of
    /// a fresh entity (<see cref="MathS.FromString(string, bool)"/> with the cache off).
    /// </summary>
    /// <example>
    /// <code>
    /// using var proof = ProofRecording.Start();
    /// var verdict = "forall n in ZZ+ : sum(1/(k (k + 1)), k, 1, n) = n/(n + 1)".ToEntity().Evaled;
    /// foreach (var step in proof.Steps)
    ///     Console.WriteLine(step);
    /// </code>
    /// </example>
    public sealed class ProofRecording : IDisposable
    {
        [ConcurrentField]
        private static readonly AsyncLocal<ProofRecording?> current = new();

        private readonly ProofRecording? enclosing;

        private readonly List<ProofStep> steps = new();

        private readonly object gate = new();

        private volatile bool closed;

        [ThreadStatic] private static int depth;

        private ProofRecording(ProofRecording? enclosing) => this.enclosing = enclosing;

        /// <summary>Opens a scope; dispose it to close. Scopes nest, and each collects its own steps.</summary>
        public static ProofRecording Start()
        {
            var recording = new ProofRecording(current.Value);
            current.Value = recording;
            return recording;
        }

        /// <summary>The steps recorded so far, in the order they were decided: a statement's sub-steps come before it.</summary>
        public IReadOnlyList<ProofStep> Steps
        {
            get { lock (gate) return steps.ToArray(); }
        }

        /// <summary>
        /// The steps written as the reference writes a proof: the statement asked last, its
        /// sub-steps indented under the order they were needed in.
        /// </summary>
        public string Written()
        {
            var builder = new StringBuilder();
            foreach (var step in Steps)
                builder.AppendLine(step.ToString());
            return builder.ToString();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (closed)
                return;
            closed = true;
            if (ReferenceEquals(current.Value, this))
                current.Value = enclosing;
        }

        internal static ProofRecording? Current => current.Value;

        /// <summary>Whether any scope is open on this flow, read once per decision.</summary>
        internal static bool Recording => current.Value is { closed: false };

        internal static void Enter() => depth++;

        internal static void Leave() => depth--;

        internal static void Add(Entity statement, string rule, string lemma, Entity verdict)
            => Add(statement, rule, lemma, verdict, depth - 1);

        /// <summary>A step one below the decision in progress: a base case evaluated inside an induction.</summary>
        internal static void AddBelow(Entity statement, string rule, string lemma, Entity verdict)
            => Add(statement, rule, lemma, verdict, depth);

        private static void Add(Entity statement, string rule, string lemma, Entity verdict, int at)
        {
            var recording = current.Value;
            if (recording is null || recording.closed)
                return;
            lock (recording.gate)
                recording.steps.Add(new ProofStep(statement, rule, lemma, verdict, at));
        }

        /// <summary>
        /// Where the steps stand now, so that an attempt which comes to nothing can be taken
        /// back with <see cref="Rollback"/>: what a route tried and abandoned is not part of
        /// the proof of what it found.
        /// </summary>
        internal static int Mark()
        {
            var recording = current.Value;
            if (recording is null || recording.closed)
                return 0;
            lock (recording.gate)
                return recording.steps.Count;
        }

        internal static void Rollback(int mark)
        {
            var recording = current.Value;
            if (recording is null || recording.closed)
                return;
            lock (recording.gate)
                if (recording.steps.Count > mark)
                    recording.steps.RemoveRange(mark, recording.steps.Count - mark);
        }
    }
}
