//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;

namespace AngouriMath.Core.Budgets
{
    /// <summary>
    /// What a bounded computation spent, and what stopped it if anything did.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The point of it is that <see cref="Reason"/> survives. A computation that gives up
    /// on a resource and a computation that concluded there is nothing to find both hand the
    /// caller nothing, and the library has had no way to tell those apart —
    /// <a href="https://github.com/asc-community/AngouriMath/issues/896">#896</a> is that
    /// gap reported as a defect. An outcome is the "because" that goes with the shrug.
    /// </para>
    /// <para>
    /// It does not say what to do next, and deliberately makes no attempt to. Which ceiling
    /// fired was measured against whether the fall-through then terminates, on
    /// <a href="https://github.com/asc-community/AngouriMath/issues/896">#896</a>'s own
    /// corpus, and the same ceiling appears on both sides: two systems the fall-through
    /// solves in seconds and one it cannot finish at all all decline on the quotient
    /// dimension. So this is a report, not a routing decision.
    /// </para>
    /// </remarks>
    /// <param name="Where">
    /// Which computation this is about, for a reader — not a stable identifier to switch on.
    /// </param>
    /// <param name="Reason">
    /// What stopped the computation — a ceiling it reached, or a shape it could not work
    /// with — or <see langword="null"/> where it ran to its own end. Named rather than
    /// enumerated because most of these belong to one algorithm and mean nothing outside it.
    /// </param>
    /// <param name="Steps">Units of work charged.</param>
    /// <param name="Elapsed">How long it took, whether or not a clock bounded it.</param>
    /// <param name="IsDeterministic">
    /// Whether this outcome is a function of the input alone. False exactly when a wall-clock
    /// ceiling is what stopped it, in which case a faster machine would have got further.
    /// </param>
    public sealed record BudgetOutcome(
        string Where, string? Reason, long Steps, TimeSpan Elapsed, bool IsDeterministic)
    {
        /// <summary>Whether the computation ran to its own end rather than being stopped.</summary>
        public bool Completed => Reason is null;

        /// <inheritdoc/>
        public override string ToString()
            => Completed
                ? $"{Where}: completed, {Steps} steps, {Elapsed.TotalMilliseconds:F1} ms"
                : $"{Where}: gave up on {Reason}, {Steps} steps, {Elapsed.TotalMilliseconds:F1} ms"
                    + (IsDeterministic ? "" : " (machine-dependent)");
    }
}
