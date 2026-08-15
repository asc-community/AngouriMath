//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Concurrent;
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
    /// <b>What this is not.</b> It records rewrites — which is what
    /// <a href="https://github.com/asc-community/AngouriMath/issues/28">#28</a> asks for —
    /// and not everything <see cref="Entity.Simplify(int)"/> does. Simplification also
    /// expands, factorises, divides polynomials, minimises boolean expressions and then
    /// *chooses* among the candidates by a complexity metric; the steps below are the
    /// rewrites, in the order they fired, across every candidate that was generated,
    /// including the ones that lost. Reading them as a route from the input to the returned
    /// answer would be reading in something that is not there.
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
        }
    }
}
