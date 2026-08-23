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

namespace AngouriMath.Core.Budgets
{
    /// <summary>
    /// Collects what each bounded computation spent while it is open, so that an answer can
    /// be asked why it stopped rather than only what it is.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A scope rather than a setting, and off unless asked for. With no recording open, a
    /// bounded computation costs one ambient read more than it did — per computation, not
    /// per step — and allocates nothing. A caller who never mentions budgets pays for none
    /// of this, which is the condition
    /// <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a> puts on
    /// anything added above the tree.
    /// </para>
    /// <para>
    /// It exists because the reason has further to travel than the return value does.
    /// <c>Solve</c> hands back a set; a set has no room in it for "the Gröbner path declined
    /// on the quotient dimension and the fall-through answered instead", and widening every
    /// signature between here and there to carry a reason nobody usually wants is a poor
    /// trade. A scope reaches the caller across those signatures without changing any of
    /// them.
    /// </para>
    /// <para>
    /// Per flow, like <see cref="MathS.Settings"/> and
    /// <see cref="AngouriMath.Core.Transformations.RewriteRecording"/>: the recording
    /// belongs to the call rather than to the thread running it, so it survives an
    /// <see langword="await"/>, and a recording opened inside a task is invisible to that
    /// task's siblings. Order across flows is not guaranteed; within one it is the order
    /// the computations finished in.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// using AngouriMath;
    /// using AngouriMath.Core.Budgets;
    ///
    /// using var recording = BudgetRecording.Start();
    /// var solutions = MathS.Equations("x2 + y2 - 4", "x y - 1").Solve("x", "y");
    /// foreach (var outcome in recording.Outcomes)
    ///     Console.WriteLine(outcome);
    /// </code>
    /// </example>
    public sealed class BudgetRecording : IDisposable
    {
        /// <summary>
        /// Which recording the current call reports to. An <see cref="AsyncLocal{T}"/> rather
        /// than <c>[ThreadStatic]</c>, so the scope follows the call rather than the thread
        /// that happens to be running it.
        /// </summary>
        [ConcurrentField]
        private static readonly AsyncLocal<BudgetRecording?> current = new();

        private readonly BudgetRecording? enclosing;

        /// <summary>
        /// Concurrent because the pointer above flows into child tasks, so two of them can
        /// report to one recording at once.
        /// </summary>
        private readonly ConcurrentQueue<BudgetOutcome> outcomes = new();

        private volatile bool closed;

        private BudgetRecording(BudgetRecording? enclosing) => this.enclosing = enclosing;

        /// <summary>
        /// Opens a recording for this call. Dispose it to close it — the value is meant to be
        /// held in a <see langword="using"/>, as <see cref="MathS.Settings"/> values are.
        /// </summary>
        /// <remarks>
        /// Recordings nest: opening one inside another hides the outer one until the inner is
        /// disposed, so a caller who records a subcomputation does not silently add its
        /// outcomes to somebody else's list.
        /// </remarks>
        public static BudgetRecording Start()
        {
            var recording = new BudgetRecording(current.Value);
            current.Value = recording;
            return recording;
        }

        /// <summary>
        /// What each bounded computation spent while this recording was open, in the order
        /// they finished. A snapshot taken when you ask, not a live view, since the
        /// underlying store has to tolerate concurrent writers.
        /// </summary>
        public IReadOnlyList<BudgetOutcome> Outcomes => outcomes.ToArray();

        /// <summary>
        /// The outcomes that stopped short — the ones with a reason to report.
        /// </summary>
        public IEnumerable<BudgetOutcome> Exhausted
        {
            get
            {
                foreach (var outcome in outcomes)
                    if (!outcome.Completed)
                        yield return outcome;
            }
        }

        internal static void Report(BudgetLedger ledger)
        {
            if (current.Value is { closed: false } recording)
                recording.outcomes.Enqueue(ledger.Outcome());
        }

        /// <summary>
        /// Closes this recording and puts the enclosing one, if there was one, back in force.
        /// Disposing twice does nothing the second time.
        /// </summary>
        public void Dispose()
        {
            if (closed)
                return;
            closed = true;
            if (ReferenceEquals(current.Value, this))
                current.Value = enclosing;
        }
    }
}
