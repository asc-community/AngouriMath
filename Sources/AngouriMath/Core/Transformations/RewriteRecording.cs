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
    /// Collects the rewrites that fire while it is open, so that an answer can be asked how
    /// it was reached rather than only what it is.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Off unless asked for, and off is free: with no recording open, applying a rule set
    /// costs one thread-static read more than it did before — per rule set, not per node —
    /// and allocates nothing. That is the condition
    /// <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a> puts on
    /// every layer above the tree, and it is why this is a scope rather than a setting that
    /// something might leave on.
    /// </para>
    /// <para>
    /// Per thread, like <see cref="MathS.Settings"/>: a recording opened on one thread does
    /// not see rewrites on another, so a parallel caller records its own work and nobody
    /// else's.
    /// </para>
    /// <para>
    /// <b>A synchronous scope, and it has to be.</b> Do not <see langword="await"/> inside
    /// one. The recording follows the thread rather than the call, so yielding lets whatever
    /// else that thread picks up be recorded as if it were yours, and the continuation may
    /// come back on a different thread than the one holding it. Closing is written to
    /// survive both — a recording closed elsewhere leaves the opening thread pointing at
    /// something that ignores what it is handed rather than at a list that keeps growing —
    /// but what gets collected in between is not something this can make meaningful. Record
    /// around synchronous work, and await outside the scope.
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
        [ThreadStatic]
        private static RewriteRecording? current;

        private readonly RewriteRecording? enclosing;
        private readonly List<RewriteStep> steps = new();
        private bool closed;

        private RewriteRecording(RewriteRecording? enclosing) => this.enclosing = enclosing;

        /// <summary>
        /// Opens a recording on this thread. Dispose it to close it — the value is meant to
        /// be held in a <see langword="using"/>, as <see cref="MathS.Settings"/> values are.
        /// </summary>
        /// <remarks>
        /// Recordings nest: opening one inside another hides the outer one until the inner
        /// is disposed, so a caller who records a subcomputation does not silently add its
        /// steps to somebody else's list.
        /// </remarks>
        public static RewriteRecording Start() => current = new RewriteRecording(current);

        /// <summary>
        /// The rewrites that fired while this recording was open, in the order they fired.
        /// </summary>
        public IReadOnlyList<RewriteStep> Steps => steps;

        /// <summary>Closes the recording. <see cref="Steps"/> stays readable afterwards.</summary>
        public void Dispose()
        {
            if (closed)
                return;
            closed = true;
            // Only this thread's chain is ours to put back. Disposing on a thread other than
            // the one that opened it -- which is what awaiting inside a recording leads to --
            // would otherwise clear whatever that thread was recording into, and leave the
            // opening thread pointing at a closed recording. Add ignores that case, so the
            // worst a stray reference can do is nothing.
            if (ReferenceEquals(current, this))
                current = enclosing;
        }

        /// <summary>
        /// The recording to report to, or <see langword="null"/> where nobody is listening —
        /// which is the case this has to stay free for.
        /// </summary>
        internal static RewriteRecording? Current => current;

        internal void Add(RewriteRuleSet ruleSet, Entity before, Entity after)
        {
            if (closed)
                return;
            steps.Add(new RewriteStep(ruleSet, before, after));
        }
    }
}
